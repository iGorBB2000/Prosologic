using Prosologic.Core.Models;

namespace Prosologic.Runtime.Services;

public class TagEngine : ITagEngine
{
    private readonly ILogger<TagEngine> _logger;
    private Project? _project;
    
    public event EventHandler<TagValueChangedEventArgs>? TagValueChanged;
    
    public TagEngine(ILogger<TagEngine> logger)
    {
        _logger = logger;
    }
    
    public Task InitializeAsync(Project project)
    {
        _project = project;
        _logger.LogInformation("Tag engine initialized");
        
        foreach (var (group, tag, path) in project.GetAllTags())
        {
            _logger.LogInformation("Tag: {Path} ({DataType}, {Strategy})", 
                path, tag.DataType, tag.UpdateStrategy);
        }
        
        return Task.CompletedTask;
    }
    
    public Task StartAsync()
    {
        _logger.LogInformation("Tag engine started (stub - no updates yet)");
        return Task.CompletedTask;
    }
    
    public Task StopAsync()
    {
        _logger.LogInformation("Tag engine stopped");
        return Task.CompletedTask;
    }
}