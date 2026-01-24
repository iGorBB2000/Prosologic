using Prosologic.Core.Models;
using Prosologic.Core.Serialization;
using ReactiveUI;
using System;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;

namespace Prosologic.Studio.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ProjectSerializer _serializer;

    private Project? _currentProject;
    private string _statusMessage = "Ready";
    private string _currentProjectPath = string.Empty;
    private bool _showContainersPanel = false;

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

    public string ShowContainersPanelText =>
        ShowContainersPanel ? "Hide Containers" : "Show Containers";

    public string WindowTitle => CurrentProject != null
        ? $"Prosologic Studio - {CurrentProject.ProjectName}"
        : "Prosologic Studio";

    public bool IsProjectOpen => CurrentProject != null;
    #endregion

    #region Commands
    public ReactiveCommand<Unit, Unit> NewProjectCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenProjectCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveProjectCommand { get; }
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

        NewProjectCommand = ReactiveCommand.Create(NewProject);
        OpenProjectCommand = ReactiveCommand.Create(OpenProject);
        ToggleContainersCommand = ReactiveCommand.Create(ToggleContainers);

        var canSave = this.WhenAnyValue(x => x.IsProjectOpen);
        SaveProjectCommand = ReactiveCommand.CreateFromTask(SaveProjectAsync, canSave);
        CloseProjectCommand = ReactiveCommand.Create(CloseProject, canSave);

        ExitCommand = ReactiveCommand.Create(Exit);

        this.WhenAnyValue(x => x.CurrentProject)
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
        StatusMessage = "New project created";

        ProjectExplorer.LoadProject(CurrentProject);
    }

    private void OpenProject()
    {
        StatusMessage = "Open project - not implemented yet";
    }

    private async Task SaveProjectAsync()
    {
        if (CurrentProject == null) return;

        try
        {
            if (string.IsNullOrEmpty(_currentProjectPath))
            {
                _currentProjectPath = Path.Combine(
                    Path.GetTempPath(),
                    "Prosologic",
                    CurrentProject.ProjectName
                );
            }

            StatusMessage = "Saving project...";
            await _serializer.SaveAsync(CurrentProject, _currentProjectPath);
            StatusMessage = "Project saved successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save failed: {ex.Message}";
        }
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

    private void ToggleContainers()
    {
        ShowContainersPanel = !ShowContainersPanel;
    }

    private void CloseProject()
    {
        CurrentProject = null;
        _currentProjectPath = string.Empty;
        StatusMessage = "Project closed";
        this.RaisePropertyChanged(nameof(IsProjectOpen));

        ProjectExplorer.Clear();
    }

    private void Exit()
    {
        Environment.Exit(0);
    }
}
