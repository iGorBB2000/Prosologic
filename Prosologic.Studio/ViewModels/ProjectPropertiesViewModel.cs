using Prosologic.Core.Enums;
using Prosologic.Core.Models;
using Prosologic.Core.Models.Mqtt;
using Prosologic.Core.Models.OpcUa;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace Prosologic.Studio.ViewModels
{
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

        public string ProjectName
        {
            get => _projectName;
            set
            {
                this.RaiseAndSetIfChanged(ref _projectName, value);
                HasChanges = true;
            }
        }

        public string Version
        {
            get => _version;
            set
            {
                this.RaiseAndSetIfChanged(ref _version, value);
                HasChanges = true;
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                this.RaiseAndSetIfChanged(ref _description, value);
                HasChanges = true;
            }
        }

        public string Author
        {
            get => _author;
            set
            {
                this.RaiseAndSetIfChanged(ref _author, value);
                HasChanges = true;
            }
        }

        public ProtocolType SelectedProtocol
        {
            get => _selectedProtocol;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedProtocol, value);
                IsMqtt = value == ProtocolType.Mqtt;
                IsOpcUa = value == ProtocolType.OpcUa;
                HasChanges = true;
            }
        }

        public List<ProtocolType> AvailableProtocols { get; } =
            Enum.GetValues<ProtocolType>().ToList();

        public bool IsMqtt
        {
            get => _isMqtt;
            set => this.RaiseAndSetIfChanged(ref _isMqtt, value);
        }

        public bool IsOpcUa
        {
            get => _isOpcUa;
            set => this.RaiseAndSetIfChanged(ref _isOpcUa, value);
        }

        public string MqttBrokerAddress
        {
            get => _mqttBrokerAddress;
            set
            {
                this.RaiseAndSetIfChanged(ref _mqttBrokerAddress, value);
                HasChanges = true;
            }
        }

        public int MqttPort
        {
            get => _mqttPort;
            set
            {
                this.RaiseAndSetIfChanged(ref _mqttPort, value);
                HasChanges = true;
            }
        }

        public string MqttClientId
        {
            get => _mqttClientId;
            set
            {
                this.RaiseAndSetIfChanged(ref _mqttClientId, value);
                HasChanges = true;
            }
        }

        public string MqttTopicPrefix
        {
            get => _mqttTopicPrefix;
            set
            {
                this.RaiseAndSetIfChanged(ref _mqttTopicPrefix, value);
                HasChanges = true;
            }
        }

        public string OpcUaServerName
        {
            get => _opcUaServerName;
            set
            {
                this.RaiseAndSetIfChanged(ref _opcUaServerName, value);
                HasChanges = true;
            }
        }

        public int OpcUaPort
        {
            get => _opcUaPort;
            set
            {
                this.RaiseAndSetIfChanged(ref _opcUaPort, value);
                HasChanges = true;
            }
        }

        public string OpcUaApplicationUri
        {
            get => _opcUaApplicationUri;
            set
            {
                this.RaiseAndSetIfChanged(ref _opcUaApplicationUri, value);
                HasChanges = true;
            }
        }

        public string OpcUaNamespaceUri
        {
            get => _opcUaNamespaceUri;
            set
            {
                this.RaiseAndSetIfChanged(ref _opcUaNamespaceUri, value);
                HasChanges = true;
            }
        }

        public bool HasChanges
        {
            get => _hasChanges;
            private set => this.RaiseAndSetIfChanged(ref _hasChanges, value);
        }

        public bool IsProjectLoaded => _currentProject != null;

        #region Commands
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetCommand { get; }
        #endregion

        public event EventHandler? ProjectSaved;

        public ProjectPropertiesViewModel()
        {
            var canSave = this.WhenAnyValue(
                x => x.IsProjectLoaded,
                x => x.HasChanges,
                (loaded, changed) => loaded && changed);

            SaveCommand = ReactiveCommand.Create(Save, canSave);
            ResetCommand = ReactiveCommand.Create(Reset, this.WhenAnyValue(x => x.IsProjectLoaded));
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

            this.RaisePropertyChanged(nameof(ProjectName));
            this.RaisePropertyChanged(nameof(Version));
            this.RaisePropertyChanged(nameof(Description));
            this.RaisePropertyChanged(nameof(Author));
            this.RaisePropertyChanged(nameof(SelectedProtocol));
            this.RaisePropertyChanged(nameof(IsMqtt));
            this.RaisePropertyChanged(nameof(IsOpcUa));
            this.RaisePropertyChanged(nameof(MqttBrokerAddress));
            this.RaisePropertyChanged(nameof(MqttPort));
            this.RaisePropertyChanged(nameof(MqttClientId));
            this.RaisePropertyChanged(nameof(MqttTopicPrefix));
            this.RaisePropertyChanged(nameof(OpcUaServerName));
            this.RaisePropertyChanged(nameof(OpcUaPort));
            this.RaisePropertyChanged(nameof(OpcUaApplicationUri));
            this.RaisePropertyChanged(nameof(OpcUaNamespaceUri));
            this.RaisePropertyChanged(nameof(IsProjectLoaded));

            HasChanges = false;
        }

        public void Clear()
        {
            _currentProject = null;
            this.RaisePropertyChanged(nameof(IsProjectLoaded));
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
            if (_currentProject == null) return;
            LoadProject(_currentProject);
        }
    }
}
