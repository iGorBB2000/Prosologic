using Prosologic.Core.Models;

namespace Prosologic.Runtime.Services;

public interface IMqttPublisher
{
    Task StartAsync(Project project);
    Task StopAsync();
    Task PublishTagValueAsync(string tagPath, object? value, DateTime timestamp);
}