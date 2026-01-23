using Prosologic.Core.Enums;

namespace Prosologic.Core.Models
{
    public abstract class ProtocolConfiguration
    {
        // Protocol Identity
        public abstract ProtocolType ProtocolType { get; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Connection Settings
        public string Host { get; set; } = "localhost";
        public int Port { get; set; }
        public bool UseTls { get; set; } = false;
        public int ConnectionTimeout { get; set; } = 5000;
        public bool AutoReconnect { get; set; } = true;
        public int ReconnectDelay { get; set; } = 5000;

        // Authentication
        public bool RequiresAuthentication { get; set; } = false;
        public string? Username { get; set; }
        public string? Password { get; set; }

        // Update Behaviour
        public bool PublishOnChange { get; set; } = true;
        public bool AllowExternalWrites { get; set; } = true;
        public int MinimumUpdateInterval { get; set; } = 0;
        public int MaxQueuedUpdates { get; set; } = 1000;

        // Data Format
        public ByteOrder ByteOrder { get; set; } = ByteOrder.LittleEndian;
        public string StringEncoding { get; set; } = "UTF-8";
        public bool IncludeTimestamp { get; set; } = true;
        public bool IncludeQuality { get; set; } = true;

        // Data Mapping
        public string AddressPrefix { get; set; } = string.Empty;
        public AddressMappingStrategy MappingStrategy { get; set; } = AddressMappingStrategy.Hierarchical;
        public string PathSeparator { get; set; } = "/";

        public abstract int GetDefaultPort();

        public abstract string MapTagPath(string tagPath);

        public virtual ValidationResult Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Host))
                errors.Add("Host address is required");

            if (Port < 1 || Port > 65535)
                errors.Add("Port must be between 1 and 65535");

            if (RequiresAuthentication && string.IsNullOrWhiteSpace(Username))
                errors.Add("Username is required when authentication is enabled");

            if (ConnectionTimeout < 100)
                errors.Add("Connection timeout must be at least 100ms");

            if (ReconnectDelay < 100)
                errors.Add("Reconnect delay must be at least 100ms");

            ValidateProtocolSpecific(errors);

            return errors.Any()
                ? ValidationResult.Failure(errors.ToArray())
                : ValidationResult.Success();
        }

        protected virtual void ValidateProtocolSpecific(List<string> errors) { }
    }
}
