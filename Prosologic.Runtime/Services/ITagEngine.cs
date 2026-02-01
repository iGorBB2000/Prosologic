using Prosologic.Core.Models;

namespace Prosologic.Runtime.Services;

public interface ITagEngine
{
    Task InitializeAsync(Project project);
    Task StartAsync();
    Task StopAsync();
    
    event EventHandler<TagValueChangedEventArgs>? TagValueChanged;
}