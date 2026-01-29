using System.Text.Json;
using MQTTnet;
using MQTTnet.Diagnostics.Logger;
using MQTTnet.Implementations;
using Prosologic.Core.Models;
using Prosologic.Core.Models.Mqtt;

namespace Prosologic.Runtime.Services;

public class MqttPublisher : IMqttPublisher
{
    private readonly ILogger<MqttPublisher> _logger;
    private IMqttClient? _mqttClient;
    private MqttConfiguration? _config;
    private string _topicPrefix = string.Empty;
    private bool _isConnected;
    
    public MqttPublisher(ILogger<MqttPublisher> logger)
    {
        _logger = logger;
    }
    
    public async Task StartAsync(Project project)
    {
        if (project.Protocol is not MqttConfiguration mqttConfig)
        {
            _logger.LogWarning("Project does not use MQTT protocol, skipping MQTT publisher");
            return;
        }
        
        _config = mqttConfig;
        _topicPrefix = mqttConfig.AddressPrefix ?? project.ProjectName;
        
        _logger.LogInformation("Starting MQTT publisher...");
        _logger.LogInformation("Broker: {Broker}:{Port}", mqttConfig.Host, mqttConfig.Port);
        _logger.LogInformation("Client ID: {ClientId}", mqttConfig.ClientId);
        _logger.LogInformation("Topic prefix: {Prefix}", _topicPrefix);
        
        try
        {
            _mqttClient = new MqttClient(new MqttClientAdapterFactory(), new MqttNetEventLogger());
            
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(mqttConfig.Host, mqttConfig.Port)
                .WithClientId(mqttConfig.ClientId)
                .WithCredentials(mqttConfig.Username, mqttConfig.Password)  // Add to config
                .WithCleanSession()
                .Build();
            
            _mqttClient.DisconnectedAsync += OnDisconnectedAsync;
            _mqttClient.ConnectedAsync += OnConnectedAsync;
            
            await _mqttClient.ConnectAsync(options);
            
            _isConnected = true;
            _logger.LogInformation("MQTT publisher started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start MQTT publisher");
            throw;
        }
    }
    
    public async Task StopAsync()
    {
        if (_mqttClient == null) return;
        
        _logger.LogInformation("Stopping MQTT publisher...");
        
        try
        {
            if (_mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync();
            }
            
            _mqttClient.Dispose();
            _isConnected = false;
            
            _logger.LogInformation("MQTT publisher stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping MQTT publisher");
        }
    }
    
    public async Task PublishTagValueAsync(string tagPath, object? value, DateTime timestamp)
    {
        if (_mqttClient == null || !_isConnected)
        {
            _logger.LogWarning("MQTT client not connected, skipping publish for {TagPath}", tagPath);
            return;
        }
        
        try
        {
            var topic = $"{_topicPrefix}/{tagPath}";
            
            var payload = new
            {
                value = value,
                timestamp = timestamp.ToString("O"), // ISO 8601 format
                quality = "good"
            };
            
            var json = JsonSerializer.Serialize(payload);
            
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(json)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(true)
                .Build();
            
            await _mqttClient.PublishAsync(message);
            
            _logger.LogDebug("Published to {Topic}: {Value}", topic, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish tag value for {TagPath}", tagPath);
        }
    }
    
    private Task OnConnectedAsync(MqttClientConnectedEventArgs args)
    {
        _logger.LogInformation("Connected to MQTT broker");
        return Task.CompletedTask;
    }
    
    private async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        _logger.LogWarning("Disconnected from MQTT broker: {Reason}", args.Reason);
        
        if (_config != null)
        {
            _logger.LogInformation("Attempting to reconnect in 5 seconds...");
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            try
            {
                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(_config.Host, _config.Port)
                    .WithClientId(_config.ClientId)
                    .WithCleanSession()
                    .Build();
                
                await _mqttClient!.ConnectAsync(options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reconnection failed");
            }
        }
    }
}