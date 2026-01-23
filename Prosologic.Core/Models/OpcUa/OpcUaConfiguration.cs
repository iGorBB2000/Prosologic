using Prosologic.Core.Enums;
using Prosologic.Core.Enums.OpcUa;

namespace Prosologic.Core.Models.OpcUa
{
    public class OpcUaConfiguration : ProtocolConfiguration
    {
        public override ProtocolType ProtocolType => ProtocolType.OpcUa;

        public string ServerName { get; set; } = "Prosologic Simulator";
        public string ApplicationUri { get; set; } = "urn:prosologic:simulator";
        public string NamespaceUri { get; set; } = "http://prosologic.io";
        public string? ProductUri { get; set; }
        public string ManufacturerName { get; set; } = "Prosologic";
        public string ProductName { get; set; } = "Prosologic PLC Simulator";
        public string SoftwareVersion { get; set; } = "1.0.0";

        public string BindAddress { get; set; } = "0.0.0.0";
        public int MaxConnections { get; set; } = 100;
        public int SessionTimeout { get; set; } = 30000;


        public OpcUaSecurityMode SecurityMode { get; set; } = OpcUaSecurityMode.None;
        public string SecurityPolicy { get; set; } = "None";
        public bool AllowAnonymous { get; set; } = true;
        public List<OpcUaUserIdentity>? AllowedUsers { get; set; }
        public string? ServerCertificatePath { get; set; }
        public string? CertificateStorePath { get; set; }


        public NodeIdStrategy NodeIdStrategy { get; set; } = NodeIdStrategy.StringPath;
        public bool CreateFolderStructure { get; set; } = true;
        public string? BaseNodeId { get; set; }
        public string? RootFolderNodeId { get; set; }
        public string RootFolderName { get; set; } = "Prosologic";

        public int DefaultPublishingInterval { get; set; } = 1000;
        public int MinPublishingInterval { get; set; } = 100;
        public int MaxPublishingInterval { get; set; } = 10000;
        public int DefaultSamplingInterval { get; set; } = 100;
        public uint DefaultQueueSize { get; set; } = 10;
        public bool DiscardOldest { get; set; } = true;

        public bool EnableHistoricalAccess { get; set; } = false;
        public int MaxBrowseResults { get; set; } = 1000;
        public int MaxReferences { get; set; } = 100;

        public override int GetDefaultPort() => 4840;

        protected override void ValidateProtocolSpecific(List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(ServerName))
                errors.Add("OPC UA server name is required");

            if (string.IsNullOrWhiteSpace(ApplicationUri))
                errors.Add("OPC UA application URI is required");

            if (string.IsNullOrWhiteSpace(NamespaceUri))
                errors.Add("OPC UA namespace URI is required");

            if (MaxConnections < 1)
                errors.Add("Maximum connections must be at least 1");

            if (SessionTimeout < 1000)
                errors.Add("Session timeout must be at least 1000ms");

            if (DefaultPublishingInterval < MinPublishingInterval)
                errors.Add("Default publishing interval cannot be less than minimum");

            if (DefaultPublishingInterval > MaxPublishingInterval)
                errors.Add("Default publishing interval cannot be greater than maximum");
        }

        public override string MapTagPath(string tagPath)
        {
            switch (NodeIdStrategy)
            {
                case NodeIdStrategy.StringPath:
                    var path = string.IsNullOrEmpty(AddressPrefix)
                        ? tagPath
                        : $"{AddressPrefix}/{tagPath}";
                    return $"ns=2;s={path}";

                case NodeIdStrategy.NumericHash:
                    var hash = Math.Abs(tagPath.GetHashCode());
                    return $"ns=2;i={hash}";

                case NodeIdStrategy.NumericSequence:
                    return $"ns=2;i={Math.Abs(tagPath.GetHashCode())}";

                case NodeIdStrategy.Guid:
                    var guid = GenerateDeterministicGuid(tagPath);
                    return $"ns=2;g={guid}";

                default:
                    return $"ns=2;s={tagPath}";
            }
        }

        private Guid GenerateDeterministicGuid(string input)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return new Guid(hash);
        }
    }
}
