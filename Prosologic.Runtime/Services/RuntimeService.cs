namespace Prosologic.Runtime.Services;

public class RuntimeService : BackgroundService
{
    private readonly ILogger<RuntimeService> _logger;
    private readonly IProjectLoader _projectLoader;
    private readonly ITagEngine _tagEngine;
    private readonly RuntimeOptions _options;
    
    public RuntimeService(
        ILogger<RuntimeService> logger,
        IProjectLoader projectLoader,
        ITagEngine tagEngine,
        RuntimeOptions options)
    {
        _logger = logger;
        _projectLoader = projectLoader;
        _tagEngine = tagEngine;
        _options = options;
        
        _logger.LogInformation("RuntimeOptions.ProjectPath = '{ProjectPath}'", _options.ProjectPath);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Runtime service starting...");
            
            if (string.IsNullOrEmpty(_options.ProjectPath))
            {
                _logger.LogError("Project path is null or empty!");
                throw new InvalidOperationException("Project path not configured");
            }
            
            var project = await _projectLoader.LoadProjectAsync(_options.ProjectPath);
            await _tagEngine.InitializeAsync(project);
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
            await _tagEngine.StopAsync();
            _logger.LogInformation("Runtime service stopped");
        }
    }
}