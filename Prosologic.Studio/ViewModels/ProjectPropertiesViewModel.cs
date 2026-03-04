using CommunityToolkit.Mvvm.Input;
using Prosologic.Core.Enums;
using Prosologic.Core.Models;
using Prosologic.Core.Models.Mqtt;
using Prosologic.Core.Models.OpcUa;

namespace Prosologic.Studio.ViewModels;

public class ProjectPropertiesViewModel : ViewModelBase
{
    private Project? _currentProject;
    private bool _hasChanges;

    private string _projectName = string.Empty;
    private string _version = "1.0.0";
    private string _description = string.Empty;
    private string _author = string.Empty;

    private ProtocolType _selectedProtocol = ProtocolType.Mqtt;
    private bool _isMqtt = true;
    private bool _isOpcUa = false;

    private string _mqttBrokerAddress = "localhost";
    private int _mqttPort = 1883;
    private string _mqttClientId = "prosologic";
    private string _mqttTopicPrefix = "factory";

    private string _opcUaServerName = "Prosologic PLC";
    private int _opcUaPort = 4840;
    private string _opcUaApplicationUri = "urn:prosologic:plc";
    private string _opcUaNamespaceUri = "http://prosologic.com/plc";

    // ── General ──────────────────────────────────────────────────────────────

    public string ProjectName
    {
        get => _projectName;
        set { SetProperty(ref _projectName, value); HasChanges = true; }
    }

    public string Version
    {
        get => _version;
        set { SetProperty(ref _version, value); HasChanges = true; }
    }

    public string Description
    {
        get => _description;
        set { SetProperty(ref _description, value); HasChanges = true; }
    }

    public string Author
    {
        get => _author;
        set { SetProperty(ref _author, value); HasChanges = true; }
    }

    // ── Protocol ─────────────────────────────────────────────────────────────

    public ProtocolType SelectedProtocol
    {
        get => _selectedProtocol;
        set
        {
            SetProperty(ref _selectedProtocol, value);
            IsMqtt = value == ProtocolType.Mqtt;
            IsOpcUa = value == ProtocolType.OpcUa;
            HasChanges = true;
        }
    }

    public List<ProtocolType> AvailableProtocols { get; } = Enum.GetValues<ProtocolType>().ToList();

    public bool IsMqtt
    {
        get => _isMqtt;
        set => SetProperty(ref _isMqtt, value);
    }

    public bool IsOpcUa
    {
        get => _isOpcUa;
        set => SetProperty(ref _isOpcUa, value);
    }

    // ── MQTT ─────────────────────────────────────────────────────────────────

    public string MqttBrokerAddress
    {
        get => _mqttBrokerAddress;
        set { SetProperty(ref _mqttBrokerAddress, value); HasChanges = true; }
    }

    public int MqttPort
    {
        get => _mqttPort;
        set { SetProperty(ref _mqttPort, value); HasChanges = true; }
    }

    public string MqttClientId
    {
        get => _mqttClientId;
        set { SetProperty(ref _mqttClientId, value); HasChanges = true; }
    }

    public string MqttTopicPrefix
    {
        get => _mqttTopicPrefix;
        set { SetProperty(ref _mqttTopicPrefix, value); HasChanges = true; }
    }

    // ── OPC UA ───────────────────────────────────────────────────────────────

    public string OpcUaServerName
    {
        get => _opcUaServerName;
        set { SetProperty(ref _opcUaServerName, value); HasChanges = true; }
    }

    public int OpcUaPort
    {
        get => _opcUaPort;
        set { SetProperty(ref _opcUaPort, value); HasChanges = true; }
    }

    public string OpcUaApplicationUri
    {
        get => _opcUaApplicationUri;
        set { SetProperty(ref _opcUaApplicationUri, value); HasChanges = true; }
    }

    public string OpcUaNamespaceUri
    {
        get => _opcUaNamespaceUri;
        set { SetProperty(ref _opcUaNamespaceUri, value); HasChanges = true; }
    }

    // ── State ────────────────────────────────────────────────────────────────

    public bool HasChanges
    {
        get => _hasChanges;
        private set
        {
            SetProperty(ref _hasChanges, value);
            SaveCommand.NotifyCanExecuteChanged();
            ResetCommand.NotifyCanExecuteChanged();
        }
    }

    public bool IsProjectLoaded => _currentProject != null;

    public RelayCommand SaveCommand { get; }
    public RelayCommand ResetCommand { get; }

    public event EventHandler? ProjectSaved;

    public ProjectPropertiesViewModel()
    {
        SaveCommand = new RelayCommand(Save, () => IsProjectLoaded && HasChanges);
        ResetCommand = new RelayCommand(Reset, () => IsProjectLoaded);
    }

    public void LoadProject(Project project)
    {
        _currentProject = project;

        _projectName = project.ProjectName;
        _version = project.Version;
        _description = project.Description ?? string.Empty;
        _author = project.Author ?? string.Empty;

        if (project.Protocol is MqttConfiguration mqtt)
        {
            _selectedProtocol = ProtocolType.Mqtt;
            _isMqtt = true;
            _isOpcUa = false;
            _mqttBrokerAddress = mqtt.Host;
            _mqttPort = mqtt.Port;
            _mqttClientId = mqtt.ClientId;
            _mqttTopicPrefix = mqtt.AddressPrefix ?? string.Empty;
        }
        else if (project.Protocol is OpcUaConfiguration opcua)
        {
            _selectedProtocol = ProtocolType.OpcUa;
            _isMqtt = false;
            _isOpcUa = true;
            _opcUaServerName = opcua.ServerName;
            _opcUaPort = opcua.Port;
            _opcUaApplicationUri = opcua.ApplicationUri;
            _opcUaNamespaceUri = opcua.NamespaceUri;
        }

        OnPropertyChanged(nameof(ProjectName));
        OnPropertyChanged(nameof(Version));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(Author));
        OnPropertyChanged(nameof(SelectedProtocol));
        OnPropertyChanged(nameof(IsMqtt));
        OnPropertyChanged(nameof(IsOpcUa));
        OnPropertyChanged(nameof(MqttBrokerAddress));
        OnPropertyChanged(nameof(MqttPort));
        OnPropertyChanged(nameof(MqttClientId));
        OnPropertyChanged(nameof(MqttTopicPrefix));
        OnPropertyChanged(nameof(OpcUaServerName));
        OnPropertyChanged(nameof(OpcUaPort));
        OnPropertyChanged(nameof(OpcUaApplicationUri));
        OnPropertyChanged(nameof(OpcUaNamespaceUri));
        OnPropertyChanged(nameof(IsProjectLoaded));

        HasChanges = false;
    }

    public void Clear()
    {
        _currentProject = null;
        OnPropertyChanged(nameof(IsProjectLoaded));
        HasChanges = false;
    }

    private void Save()
    {
        if (_currentProject == null) return;

        _currentProject.ProjectName = ProjectName;
        _currentProject.Version = Version;
        _currentProject.Description = string.IsNullOrWhiteSpace(Description) ? null : Description;
        _currentProject.Author = string.IsNullOrWhiteSpace(Author) ? null : Author;

        if (SelectedProtocol == ProtocolType.Mqtt)
        {
            var mqtt = _currentProject.Protocol as MqttConfiguration ?? new MqttConfiguration();
            mqtt.Host = MqttBrokerAddress;
            mqtt.Port = MqttPort;
            mqtt.ClientId = MqttClientId;
            mqtt.AddressPrefix = string.IsNullOrWhiteSpace(MqttTopicPrefix) ? null : MqttTopicPrefix;
            _currentProject.Protocol = mqtt;
        }
        else if (SelectedProtocol == ProtocolType.OpcUa)
        {
            var opcua = _currentProject.Protocol as OpcUaConfiguration ?? new OpcUaConfiguration();
            opcua.ServerName = OpcUaServerName;
            opcua.Port = OpcUaPort;
            opcua.ApplicationUri = OpcUaApplicationUri;
            opcua.NamespaceUri = OpcUaNamespaceUri;
            _currentProject.Protocol = opcua;
        }

        HasChanges = false;
        ProjectSaved?.Invoke(this, EventArgs.Empty);
    }

    private void Reset()
    {
        if (_currentProject != null) LoadProject(_currentProject);
    }
}