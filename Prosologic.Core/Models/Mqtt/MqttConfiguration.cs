using Prosologic.Core.Enums;
using Prosologic.Core.Enums.Mqtt;

namespace Prosologic.Core.Models.Mqtt
{
    public class MqttConfiguration : ProtocolConfiguration
    {
        public override ProtocolType ProtocolType => ProtocolType.Mqtt;

        public string ClientId { get; set; } = string.Empty;
        public bool CleanSession { get; set; } = true;
        public int KeepAliveInterval { get; set; } = 60;
        public MqttVersion ProtocolVersion { get; set; } = MqttVersion.V311;
        public MqttQosLevel DefaultQos { get; set; } = MqttQosLevel.AtLeastOnce;
        public bool DefaultRetain { get; set; } = false;
        public bool SubscribeToWrites { get; set; } = true;
        public string WriteTopicSuffix { get; set; } = "/set";

        public override int GetDefaultPort() => UseTls ? 8883 : 1883;

        protected override void ValidateProtocolSpecific(List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(ClientId))
                errors.Add("MQTT client ID is required");

            if (KeepAliveInterval < 0)
                errors.Add("Keep-alive interval cannot be negative");
        }

        public override string MapTagPath(string tagPath)
        {
            var topic = tagPath;

            if (MappingStrategy == AddressMappingStrategy.Flat)
            {
                topic = tagPath.Replace('/', '_');
            }

            if (PathSeparator != "/")
            {
                topic = topic.Replace('/', PathSeparator[0]);
            }

            if (!string.IsNullOrEmpty(AddressPrefix))
            {
                topic = $"{AddressPrefix.TrimEnd('/')}/{topic}";
            }

            return topic;
        }
    }
}
