using Prosologic.Core.Enums;
using Prosologic.Core.Models;

namespace Prosologic.Runtime.Services;

public class TagEngine : ITagEngine
{
    private readonly ILogger<TagEngine> _logger;
    private Project? _project;
    private readonly List<TagContext> _tagContexts = new();
    private bool _isRunning;
    
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
            var context = new TagContext
            {
                Tag = tag,
                Path = path,
                CurrentValue = tag.InitialValue,
                LastUpdate = DateTime.UtcNow
            };
            
            _tagContexts.Add(context);
            
            _logger.LogInformation("Tag: {Path} ({DataType}, {Strategy}, {Interval}ms)", 
                path, tag.DataType, tag.UpdateStrategy, tag.UpdateInterval);
        }
        
        return Task.CompletedTask;
    }
    
    public Task StartAsync()
    {
        _isRunning = true;
        _logger.LogInformation("Tag engine starting...");
        
        foreach (var context in _tagContexts)
        {
            StartTagTimer(context);
        }
        
        _logger.LogInformation("Tag engine started - {Count} tags updating", _tagContexts.Count);
        return Task.CompletedTask;
    }
    
    public Task StopAsync()
    {
        _isRunning = false;
        _logger.LogInformation("Tag engine stopping...");
        
        foreach (var context in _tagContexts)
        {
            context.UpdateTimer?.Dispose();
            context.UpdateTimer = null;
        }
        
        _logger.LogInformation("Tag engine stopped");
        return Task.CompletedTask;
    }
    
    private void StartTagTimer(TagContext context)
    {
        if (context.Tag.UpdateStrategy == UpdateStrategy.Static)
        {
            _logger.LogDebug("Tag {Path} is static - no timer needed", context.Path);
            
            UpdateTag(context);
            return;
        }
        
        var interval = TimeSpan.FromMilliseconds(context.Tag.UpdateInterval);
        
        context.UpdateTimer = new Timer(
            callback: _ => UpdateTag(context),
            state: null,
            dueTime: TimeSpan.Zero, // Update immediately
            period: interval
        );
        
        _logger.LogDebug("Started timer for {Path} - updates every {Interval}ms", 
            context.Path, context.Tag.UpdateInterval);
    }
    
    private void UpdateTag(TagContext context)
    {
        if (!_isRunning) return;
        
        try
        {
            object? newValue = context.Tag.UpdateStrategy switch
            {
                UpdateStrategy.Static => context.Tag.InitialValue,
                UpdateStrategy.Random => context.GenerateRandomValue(),
                UpdateStrategy.ScriptDriven => ExecuteScript(context), // Phase 3
                _ => context.CurrentValue
            };
            
            var oldValue = context.CurrentValue;
            context.CurrentValue = newValue;
            context.LastUpdate = DateTime.UtcNow;
            
            _logger.LogInformation("[{Path}] {OldValue} → {NewValue}", 
                context.Path, 
                FormatValue(oldValue), 
                FormatValue(newValue));
            
            TagValueChanged?.Invoke(this, new TagValueChangedEventArgs
            {
                TagPath = context.Path,
                Value = newValue,
                Timestamp = context.LastUpdate
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tag {Path}", context.Path);
        }
    }
    
    private object? ExecuteScript(TagContext context)
    {
        // Placeholder
        _logger.LogWarning("Script execution not implemented yet for {Path}", context.Path);
        return context.CurrentValue;
    }
    
    private string FormatValue(object? value)
    {
        if (value == null) return "null";
        
        return value switch
        {
            float f => f.ToString("F2"),
            double d => d.ToString("F2"),
            DateTime dt => dt.ToString("HH:mm:ss"),
            _ => value.ToString() ?? "null"
        };
    }
}