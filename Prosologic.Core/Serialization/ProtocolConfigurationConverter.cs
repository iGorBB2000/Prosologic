using Prosologic.Core.Models;
using Prosologic.Core.Models.Mqtt;
using Prosologic.Core.Models.OpcUa;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Prosologic.Core.Serialization
{
    public class ProtocolConfigurationConverter : JsonConverter<ProtocolConfiguration>
    {
        public override ProtocolConfiguration? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            if (!root.TryGetProperty("protocolType", out var typeProperty))
            {
                throw new JsonException("Protocol configuration must have 'protocolType' property");
            }

            var protocolType = typeProperty.GetString()?.ToLowerInvariant();

            return protocolType switch
            {
                "mqtt" => JsonSerializer.Deserialize<MqttConfiguration>(root.GetRawText(), options),
                "opcua" => JsonSerializer.Deserialize<OpcUaConfiguration>(root.GetRawText(), options),
                _ => throw new JsonException($"Unknown protocol type: {protocolType}")
            };
        }

        public override void Write(
            Utf8JsonWriter writer,
            ProtocolConfiguration value,
            JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}
