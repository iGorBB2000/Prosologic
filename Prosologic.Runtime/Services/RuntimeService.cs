using Prosologic.Core.Models;

namespace Prosologic.Runtime.Services;

public class RuntimeService : BackgroundService
{
    private readonly ILogger<RuntimeService> _logger;
    private readonly IProjectLoader _projectLoader;
    private readonly ITagEngine _tagEngine;
    private readonly IMqttPublisher _mqttPublisher;
    private readonly RuntimeOptions _options;
    
    public RuntimeService(
        ILogger<RuntimeService> logger,
        IProjectLoader projectLoader,
        ITagEngine tagEngine,
        IMqttPublisher mqttPublisher,
        RuntimeOptions options)
    {
        _logger = logger;
        _projectLoader = projectLoader;
        _tagEngine = tagEngine;
        _mqttPublisher = mqttPublisher;
        _options = options;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Runtime service starting...");
            
            var project = await _projectLoader.LoadProjectAsync(_options.ProjectPath);
            await _tagEngine.InitializeAsync(project);
            _tagEngine.TagValueChanged += OnTagValueChanged;
            await _mqttPublisher.StartAsync(project);
            await _tagEngine.StartAsync();
            
            _logger.LogInformation("Runtime service started successfully");
            _logger.LogInformation("Press Ctrl+C to stop");
            
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Runtime service stopping...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Runtime service failed");
            throw;
        }
        finally
        {
            _tagEngine.TagValueChanged -= OnTagValueChanged;
            await _tagEngine.StopAsync();
            await _mqttPublisher.StopAsync();
            _logger.LogInformation("Runtime service stopped");
        }
    }
    
    private async void OnTagValueChanged(object? sender, TagValueChangedEventArgs e)
    {
        await _mqttPublisher.PublishTagValueAsync(e.TagPath, e.Value, e.Timestamp);
    }
}