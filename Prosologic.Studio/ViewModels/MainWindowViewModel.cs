using CommunityToolkit.Mvvm.Input;
using Prosologic.Core.Enums;
using Prosologic.Core.Models;
using Prosologic.Core.Serialization;
using Prosologic.Studio.Services;
using System.Windows;

namespace Prosologic.Studio.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ProjectSerializer _serializer;
    private FileDialogService? _fileDialogService;
    private MessageBoxService? _messageBoxService;

    private Project? _currentProject;
    private string _statusMessage = "Ready";
    private string _currentProjectPath = string.Empty;
    private bool _showContainersPanel = false;
    private bool _hasUnsavedChanges = false;

    public ProjectExplorerViewModel ProjectExplorer { get; }
    public PropertiesPanelViewModel PropertiesPanel { get; }

    // ── Properties ───────────────────────────────────────────────────────────

    public Project? CurrentProject
    {
        get => _currentProject;
        set
        {
            SetProperty(ref _currentProject, value);
            // Replaces: WhenAnyValue(x => x.CurrentProject, x => x.HasUnsavedChanges).Subscribe(...)
            // We just notify the derived properties manually here instead.
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(DisplayTitle));
            OnPropertyChanged(nameof(IsProjectOpen));
            SaveProjectCommand.NotifyCanExecuteChanged();
            SaveAsProjectCommand.NotifyCanExecuteChanged();
            CloseProjectCommand.NotifyCanExecuteChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool ShowContainersPanel
    {
        get => _showContainersPanel;
        set
        {
            SetProperty(ref _showContainersPanel, value);
            OnPropertyChanged(nameof(ShowContainersPanelText));
        }
    }

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        set
        {
            SetProperty(ref _hasUnsavedChanges, value);
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(DisplayTitle));
        }
    }

    public string ShowContainersPanelText =>
        ShowContainersPanel ? "Hide Containers" : "Show Containers";

    public string WindowTitle
    {
        get
        {
            var title = "Prosologic Studio";
            if (CurrentProject != null)
            {
                title = $"{CurrentProject.ProjectName} - {title}";
                if (HasUnsavedChanges) title = $"*{title}";
            }
            return title;
        }
    }

    /// <summary>Just the project name for the title bar centre display.
    /// Fires whenever WindowTitle fires so bindings update on save.</summary>
    public string DisplayTitle => CurrentProject?.ProjectName ?? "No project";

    public bool IsProjectOpen => CurrentProject != null;

    // ── Commands ─────────────────────────────────────────────────────────────

    public RelayCommand NewProjectCommand { get; }
    public AsyncRelayCommand OpenProjectCommand { get; }
    public AsyncRelayCommand SaveProjectCommand { get; }
    public AsyncRelayCommand SaveAsProjectCommand { get; }
    public RelayCommand CloseProjectCommand { get; }
    public RelayCommand ToggleContainersCommand { get; }
    public RelayCommand ExitCommand { get; }

    // ── Constructor ──────────────────────────────────────────────────────────

    public MainWindowViewModel()
    {
        _serializer = new ProjectSerializer();
        ProjectExplorer = new ProjectExplorerViewModel();
        PropertiesPanel = new PropertiesPanelViewModel();

        ProjectExplorer.SelectionChanged += OnSelectionChanged;
        ProjectExplorer.AddTagGroupRequested += OnAddTagGroupRequested;
        ProjectExplorer.AddTagRequested += OnAddTagRequested;
        ProjectExplorer.DeleteRequested += OnDeleteRequested;

        PropertiesPanel.TagGroupEditor.GroupNameChanged += OnTagGroupNameChanged;
        PropertiesPanel.TagGroupEditor.GroupSaved += OnTagGroupSaved;
        PropertiesPanel.ProjectProperties.ProjectSaved += OnProjectPropertiesSaved;
        PropertiesPanel.TagEditor.TagSaved += OnTagSaved;

        NewProjectCommand = new RelayCommand(NewProject);
        OpenProjectCommand = new AsyncRelayCommand(OpenProjectAsync);
        SaveProjectCommand = new AsyncRelayCommand(SaveProjectAsync, () => IsProjectOpen);
        SaveAsProjectCommand = new AsyncRelayCommand(SaveAsProjectAsync, () => IsProjectOpen);
        CloseProjectCommand = new RelayCommand(CloseProject, () => IsProjectOpen);
        ToggleContainersCommand = new RelayCommand(ToggleContainers);
        ExitCommand = new RelayCommand(Exit);
    }

    public void Initialize(FileDialogService fileDialogService, MessageBoxService messageBoxService)
    {
        _fileDialogService = fileDialogService;
        _messageBoxService = messageBoxService;
    }

    // ── Command implementations ───────────────────────────────────────────────

    private void NewProject()
    {
        CurrentProject = new Project
        {
            ProjectName = "NewProject",
            Version = "1.0.0",
            Protocol = new Core.Models.Mqtt.MqttConfiguration
            {
                Host = "localhost",
                Port = 1883,
                ClientId = "prosologic-new"
            },
            TagGroups = new()
            {
                new TagGroup
                {
                    Name = "Sensors",
                    Tags = new()
                    {
                        new Tag { Name = "Temperature", DataType = TagDataType.Float, InitialValue = 20.0 },
                        new Tag { Name = "Pressure",    DataType = TagDataType.Float, InitialValue = 101.3 }
                    }
                },
                new TagGroup
                {
                    Name = "Actuators",
                    Tags = new()
                    {
                        new Tag { Name = "ConveyorSpeed", DataType = TagDataType.Float, InitialValue = 0.0 }
                    }
                }
            }
        };

        _currentProjectPath = string.Empty;
        HasUnsavedChanges = true;
        StatusMessage = "New project created";

        ProjectExplorer.LoadProject(CurrentProject);
        PropertiesPanel.ShowProject(CurrentProject);
    }

    private async Task OpenProjectAsync()
    {
        if (_fileDialogService == null) return;

        try
        {
            var folderPath = await _fileDialogService.PickOpenFolderAsync();
            if (folderPath == null) return;

            StatusMessage = "Loading project...";
            var project = await _serializer.LoadAsync(folderPath);

            CurrentProject = project;
            _currentProjectPath = folderPath;
            HasUnsavedChanges = false;

            ProjectExplorer.LoadProject(project);
            StatusMessage = $"Project loaded: {project.ProjectName}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to open project: {ex.Message}";
            if (_messageBoxService != null)
                await _messageBoxService.ShowErrorAsync("Error Opening Project",
                    $"Could not open project:\n\n{ex.Message}");
        }
    }

    private async Task SaveProjectAsync()
    {
        if (CurrentProject == null) return;

        if (string.IsNullOrEmpty(_currentProjectPath))
        {
            await SaveAsProjectAsync();
            return;
        }

        await SaveToPathAsync(_currentProjectPath);
    }

    private async Task SaveAsProjectAsync()
    {
        if (CurrentProject == null || _fileDialogService == null) return;

        try
        {
            var folderPath = await _fileDialogService.PickSaveFolderAsync(CurrentProject.ProjectName);
            if (folderPath == null) return;

            await SaveToPathAsync(folderPath);
            _currentProjectPath = folderPath;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to save project: {ex.Message}";
        }
    }

    private async Task SaveToPathAsync(string path)
    {
        if (CurrentProject == null) return;

        try
        {
            StatusMessage = "Saving project...";
            await _serializer.SaveAsync(CurrentProject, path);
            HasUnsavedChanges = false;
            StatusMessage = "Project saved successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save failed: {ex.Message}";
        }
    }

    private async void CloseProject()
    {
        if (HasUnsavedChanges && _messageBoxService != null)
            await _messageBoxService.ShowWarningAsync("Unsaved changes",
                $"{CurrentProject?.ProjectName} has unsaved changes");

        CurrentProject = null;
        _currentProjectPath = string.Empty;
        HasUnsavedChanges = false;
        StatusMessage = "Project closed";

        ProjectExplorer.Clear();
        PropertiesPanel.Clear();
    }

    private void Exit() => Environment.Exit(0);

    private void ToggleContainers() => ShowContainersPanel = !ShowContainersPanel;

    // ── Event handlers ────────────────────────────────────────────────────────

    private void OnSelectionChanged(object? sender, (NodeType Type, object? Data) e)
    {
        switch (e.Type)
        {
            case NodeType.Project when e.Data is Project project:
                PropertiesPanel.ShowProject(project);
                StatusMessage = $"Project: {project.ProjectName}";
                break;

            case NodeType.TagGroup when e.Data is TagGroup group:
                PropertiesPanel.ShowTagGroup(group);
                StatusMessage = $"Tag Group: {group.Name}";
                break;

            case NodeType.Tag when e.Data is Tag tag:
                PropertiesPanel.ShowTag(tag);
                StatusMessage = $"Tag: {tag.Name}";
                break;

            default:
                PropertiesPanel.Clear();
                StatusMessage = "Ready";
                break;
        }
    }

    private async void OnAddTagGroupRequested(object? sender, (string Name, TreeNodeViewModel Parent) e)
    {
        if (_messageBoxService == null || CurrentProject == null) return;

        while (true)
        {
            var name = await _messageBoxService.ShowInputAsync(
                "Add Tag Group",
                "Enter tag group name:",
                "e.g., Sensors, Actuators",
                "NewGroup");

            if (string.IsNullOrWhiteSpace(name)) return;

            bool isUnique = e.Parent.NodeType == NodeType.Project
                ? CurrentProject.IsTagGroupNameUniqueAtRoot(name)
                : e.Parent.DataContext is TagGroup pg && CurrentProject.IsTagGroupNameUnique(pg, name);

            if (!isUnique)
            {
                await _messageBoxService.ShowWarningAsync("Duplicate Name",
                    $"A tag group named '{name}' already exists at this level.\n\nPlease choose a different name.");
                continue;
            }

            var tagGroup = new TagGroup { Name = name };

            if (e.Parent.NodeType == NodeType.Project && e.Parent.DataContext is Project proj)
                proj.TagGroups.Add(tagGroup);
            else if (e.Parent.NodeType == NodeType.TagGroup && e.Parent.DataContext is TagGroup parentGroup)
                parentGroup.SubGroups.Add(tagGroup);

            ProjectExplorer.AddTagGroupNode(tagGroup, e.Parent);
            HasUnsavedChanges = true;
            StatusMessage = $"Tag group '{name}' added";
            break;
        }
    }

    private async void OnAddTagRequested(object? sender, (string Name, TreeNodeViewModel Parent) e)
    {
        if (_messageBoxService == null || CurrentProject == null) return;
        if (e.Parent.DataContext is not TagGroup parentGroup) return;

        while (true)
        {
            var name = await _messageBoxService.ShowInputAsync(
                "Add Tag",
                "Enter tag name:",
                "e.g., Temperature, Pressure",
                "NewTag");

            if (string.IsNullOrWhiteSpace(name)) return;

            if (!CurrentProject.IsTagNameUniqueInGroup(parentGroup, name))
            {
                await _messageBoxService.ShowWarningAsync("Duplicate Name",
                    $"A tag named '{name}' already exists in this tag group.\n\nPlease choose a different name.");
                continue;
            }

            var tag = new Tag
            {
                Name = name,
                DataType = TagDataType.Float,
                UpdateStrategy = UpdateStrategy.Static,
                UpdateInterval = 1000,
                InitialValue = 0.0,
                AccessMode = TagAccessMode.ReadWrite
            };

            parentGroup.Tags.Add(tag);
            ProjectExplorer.AddTagNode(tag, e.Parent);
            HasUnsavedChanges = true;
            StatusMessage = $"Tag '{name}' added";
            break;
        }
    }

    private async void OnDeleteRequested(object? sender, TreeNodeViewModel node)
    {
        if (_messageBoxService == null) return;

        var itemType = node.NodeType == NodeType.Tag ? "tag" : "tag group";
        var confirmed = await _messageBoxService.ConfirmAsync("Confirm Delete",
            $"Are you sure you want to delete {itemType} '{node.Name}'?");

        if (!confirmed) return;

        if (node.NodeType == NodeType.Tag && node.DataContext is Tag tag)
            RemoveTagFromProject(CurrentProject, tag);
        else if (node.NodeType == NodeType.TagGroup && node.DataContext is TagGroup group)
            RemoveTagGroupFromProject(CurrentProject, group);

        ProjectExplorer.RemoveNode(node);
        HasUnsavedChanges = true;
        StatusMessage = $"Deleted {itemType} '{node.Name}'";
    }

    private void OnTagGroupNameChanged(object? sender, (TagGroup Group, string OldName, string NewName) e)
    {
        var node = ProjectExplorer.Nodes
            .SelectMany(GetAllNodes)
            .FirstOrDefault(n => n.DataContext == e.Group);

        if (node != null) node.Name = e.NewName;

        HasUnsavedChanges = true;
    }

    private void OnTagGroupSaved(object? sender, EventArgs e) =>
        HasUnsavedChanges = true;

    private void OnTagSaved(object? sender, EventArgs e)
    {
        ProjectExplorer.SelectedNode?.RefreshName();
        HasUnsavedChanges = true;
    }

    private void OnProjectPropertiesSaved(object? sender, EventArgs e)
    {
        HasUnsavedChanges = true;

        if (CurrentProject != null && ProjectExplorer.Nodes.Count > 0)
            ProjectExplorer.Nodes[0].Name = CurrentProject.ProjectName;

        StatusMessage = "Project properties saved";
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IEnumerable<TreeNodeViewModel> GetAllNodes(TreeNodeViewModel node)
    {
        yield return node;
        foreach (var child in node.Children.SelectMany(GetAllNodes))
            yield return child;
    }

    private static void RemoveTagFromProject(Project? project, Tag tag)
    {
        if (project == null) return;
        foreach (var group in project.TagGroups)
            if (RemoveTagFromGroup(group, tag)) return;
    }

    private static bool RemoveTagFromGroup(TagGroup group, Tag tag)
    {
        if (group.Tags.Remove(tag)) return true;
        foreach (var sub in group.SubGroups)
            if (RemoveTagFromGroup(sub, tag)) return true;
        return false;
    }

    private static void RemoveTagGroupFromProject(Project? project, TagGroup groupToRemove)
    {
        if (project == null) return;
        if (project.TagGroups.Remove(groupToRemove)) return;
        foreach (var group in project.TagGroups)
            if (RemoveTagGroupFromGroup(group, groupToRemove)) return;
    }

    private static bool RemoveTagGroupFromGroup(TagGroup parent, TagGroup target)
    {
        if (parent.SubGroups.Remove(target)) return true;
        foreach (var sub in parent.SubGroups)
            if (RemoveTagGroupFromGroup(sub, target)) return true;
        return false;
    }

    private static TagGroup? FindParentGroup(Project project, Tag tag)
    {
        foreach (var group in project.TagGroups)
        {
            var parent = FindParentGroupRecursive(group, tag);
            if (parent != null) return parent;
        }
        return null;
    }

    private static TagGroup? FindParentGroupRecursive(TagGroup group, Tag tag)
    {
        if (group.Tags.Contains(tag)) return group;
        foreach (var sub in group.SubGroups)
        {
            var parent = FindParentGroupRecursive(sub, tag);
            if (parent != null) return parent;
        }
        return null;
    }
}