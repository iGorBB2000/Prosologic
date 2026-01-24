using Prosologic.Core.Enums;
using Prosologic.Core.Models;
using Prosologic.Core.Serialization;
using Prosologic.Studio.Services;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

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
    public TagEditorViewModel TagEditor { get; }

    #region Properties
    public Project? CurrentProject
    {
        get => _currentProject;
        set => this.RaiseAndSetIfChanged(ref _currentProject, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public bool ShowContainersPanel
    {
        get => _showContainersPanel;
        set
        {
            this.RaiseAndSetIfChanged(ref _showContainersPanel, value);
            this.RaisePropertyChanged(nameof(ShowContainersPanelText));
        }
    }

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        set => this.RaiseAndSetIfChanged(ref _hasUnsavedChanges, value);
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
                if (HasUnsavedChanges)
                    title = $"*{title}";
            }
            return title;
        }
    }

    public bool IsProjectOpen => CurrentProject != null;
    #endregion

    #region Commands
    public ReactiveCommand<Unit, Unit> NewProjectCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenProjectCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveProjectCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveAsProjectCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseProjectCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleContainersCommand { get; }
    public ReactiveCommand<Unit, Unit> ExitCommand { get; }
    #endregion

    public MainWindowViewModel()
    {
        _serializer = new ProjectSerializer();
        ProjectExplorer = new ProjectExplorerViewModel();
        TagEditor = new TagEditorViewModel();

        ProjectExplorer.TagSelected += OnTagSelected;
        ProjectExplorer.AddTagGroupRequested += OnAddTagGroupRequested;
        ProjectExplorer.AddTagRequested += OnAddTagRequested;
        ProjectExplorer.DeleteRequested += OnDeleteRequested;

        TagEditor.TagSaved += OnTagSaved;

        NewProjectCommand = ReactiveCommand.Create(NewProject);
        OpenProjectCommand = ReactiveCommand.CreateFromTask(OpenProjectAsync);

        var canSave = this.WhenAnyValue(x => x.IsProjectOpen);
        SaveProjectCommand = ReactiveCommand.CreateFromTask(SaveProjectAsync, canSave);
        SaveAsProjectCommand = ReactiveCommand.CreateFromTask(SaveAsProjectAsync, canSave);
        CloseProjectCommand = ReactiveCommand.Create(CloseProject, canSave);

        ToggleContainersCommand = ReactiveCommand.Create(ToggleContainers);
        ExitCommand = ReactiveCommand.Create(Exit);

        this.WhenAnyValue(x => x.CurrentProject, x => x.HasUnsavedChanges)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(WindowTitle));
                this.RaisePropertyChanged(nameof(IsProjectOpen));
            });

        TagEditor.TagSaved += (s, e) =>
        {
            if (ProjectExplorer.SelectedNode != null)
            {
                ProjectExplorer.SelectedNode.RefreshName();
            }
        };
    }

    public void Initialize(FileDialogService fileDialogService, MessageBoxService messageBoxService)
    {
        _fileDialogService = fileDialogService;
        _messageBoxService = messageBoxService;
    }

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
                // ADD SOME TEST DATA:
                new TagGroup
                {
                    Name = "Sensors",
                    Tags = new()
                    {
                        new Tag
                        {
                            Name = "Temperature",
                            DataType = Core.Enums.TagDataType.Float,
                            InitialValue = 20.0
                        },
                        new Tag
                        {
                            Name = "Pressure",
                            DataType = Core.Enums.TagDataType.Float,
                            InitialValue = 101.3
                        }
                    }
                },
                new TagGroup
                {
                    Name = "Actuators",
                    Tags = new()
                    {
                        new Tag
                        {
                            Name = "ConveyorSpeed",
                            DataType = Core.Enums.TagDataType.Float,
                            InitialValue = 0.0
                        }
                    }
                }
            }
        };

        _currentProjectPath = string.Empty;
        HasUnsavedChanges = true;
        StatusMessage = "New project created";

        ProjectExplorer.LoadProject(CurrentProject);
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
            TagEditor.Clear();

            StatusMessage = $"Project loaded: {project.ProjectName}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to open project: {ex.Message}";
            if (_messageBoxService != null)
            {
                await _messageBoxService.ShowErrorAsync("Error Opening Project",
                    $"Could not open project:\n\n{ex.Message}");
            }
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
        if (CurrentProject == null || _fileDialogService == null)
            return;

        try
        {
            var folderPath = await _fileDialogService.PickSaveFolderAsync(CurrentProject.ProjectName);
            if (folderPath == null)
                return; // User cancelled

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
            StatusMessage = $"Project saved successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save failed: {ex.Message}";
        }
    }

    private async void CloseProject()
    {
        if (HasUnsavedChanges)
        {
            await _messageBoxService.ShowWarningAsync("Unsaved changes", $"{CurrentProject.ProjectName} has unsaved changes");
            StatusMessage = "Warning: You have unsaved changes";
        }

        CurrentProject = null;
        _currentProjectPath = string.Empty;
        HasUnsavedChanges = false;
        StatusMessage = "Project closed";

        ProjectExplorer.Clear();
        TagEditor.Clear();
    }

    private void Exit()
    {
        // TODO: Check for unsaved changes
        Environment.Exit(0);
    }

    private async void OnAddTagGroupRequested(object? sender, (string Name, TreeNodeViewModel Parent) e)
    {
        if (_messageBoxService == null || CurrentProject == null) return;

        var name = await _messageBoxService.ShowInputAsync(
            "Add Tag Group",
            "Enter tag group name:",
            "e.g., Sensors, Actuators",
            "NewGroup"
        );

        if (string.IsNullOrWhiteSpace(name)) return;

        var tagGroup = new TagGroup
        {
            Name = name,
            Description = null
        };

        if (e.Parent.NodeType == NodeType.Project && e.Parent.DataContext is Project project)
        {
            project.TagGroups.Add(tagGroup);
        }
        else if (e.Parent.NodeType == NodeType.TagGroup && e.Parent.DataContext is TagGroup parentGroup)
        {
            parentGroup.SubGroups.Add(tagGroup);
        }

        ProjectExplorer.AddTagGroupNode(tagGroup, e.Parent);

        HasUnsavedChanges = true;
        StatusMessage = $"Tag group '{name}' added";
    }

    private async void OnAddTagRequested(object? sender, (string Name, TreeNodeViewModel Parent) e)
    {
        if (_messageBoxService == null || CurrentProject == null) return;

        var name = await _messageBoxService.ShowInputAsync(
            "Add Tag",
            "Enter tag name:",
            "e.g., Temperature, Pressure",
            "NewTag"
        );

        if (string.IsNullOrWhiteSpace(name)) return;

        var tag = new Tag
        {
            Name = name,
            DataType = TagDataType.Float,
            UpdateStrategy = UpdateStrategy.Static,
            UpdateInterval = 1000,
            InitialValue = 0.0,
            AccessMode = TagAccessMode.ReadWrite
        };

        if (e.Parent.DataContext is TagGroup parentGroup)
        {
            parentGroup.Tags.Add(tag);
        }

        ProjectExplorer.AddTagNode(tag, e.Parent);

        HasUnsavedChanges = true;
        StatusMessage = $"Tag '{name}' added";
    }

    private async void OnDeleteRequested(object? sender, TreeNodeViewModel node)
    {
        if (_messageBoxService == null) return;

        var itemType = node.NodeType == NodeType.Tag ? "tag" : "tag group";
        var confirmed = await _messageBoxService.ConfirmAsync(
            "Confirm Delete",
            $"Are you sure you want to delete {itemType} '{node.Name}'?"
        );

        if (!confirmed) return;

        if (node.NodeType == NodeType.Tag && node.DataContext is Tag tag)
        {
            RemoveTagFromProject(CurrentProject, tag);
        }
        else if (node.NodeType == NodeType.TagGroup && node.DataContext is TagGroup group)
        {
            RemoveTagGroupFromProject(CurrentProject, group);
        }

        ProjectExplorer.RemoveNode(node);

        HasUnsavedChanges = true;
        StatusMessage = $"Deleted {itemType} '{node.Name}'";
    }

    private void RemoveTagFromProject(Project? project, Tag tag)
    {
        if (project == null) return;

        foreach (var group in project.TagGroups)
        {
            if (RemoveTagFromGroup(group, tag))
                return;
        }
    }

    private bool RemoveTagFromGroup(TagGroup group, Tag tag)
    {
        if (group.Tags.Contains(tag))
        {
            group.Tags.Remove(tag);
            return true;
        }

        foreach (var subGroup in group.SubGroups)
        {
            if (RemoveTagFromGroup(subGroup, tag))
                return true;
        }

        return false;
    }

    private void RemoveTagGroupFromProject(Project? project, TagGroup groupToRemove)
    {
        if (project == null) return;

        if (project.TagGroups.Contains(groupToRemove))
        {
            project.TagGroups.Remove(groupToRemove);
            return;
        }

        foreach (var group in project.TagGroups)
        {
            if (RemoveTagGroupFromGroup(group, groupToRemove))
                return;
        }
    }

    private bool RemoveTagGroupFromGroup(TagGroup parent, TagGroup groupToRemove)
    {
        if (parent.SubGroups.Contains(groupToRemove))
        {
            parent.SubGroups.Remove(groupToRemove);
            return true;
        }

        foreach (var subGroup in parent.SubGroups)
        {
            if (RemoveTagGroupFromGroup(subGroup, groupToRemove))
                return true;
        }

        return false;
    }

    private void ToggleContainers()
    {
        ShowContainersPanel = !ShowContainersPanel;
    }

    private void OnTagSelected(object? sender, Tag? tag)
    {
        if (tag != null)
        {
            TagEditor.LoadTag(tag);
            StatusMessage = $"Editing tag: {tag.Name}";
        }
        else
        {
            TagEditor.Clear();
            StatusMessage = "Ready";
        }
    }

    private void OnTagSaved(object? sender, EventArgs e)
    {
        if (ProjectExplorer.SelectedNode != null)
        {
            ProjectExplorer.SelectedNode.RefreshName();
        }
        HasUnsavedChanges = true;
    }
}
