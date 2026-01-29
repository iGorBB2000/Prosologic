namespace Prosologic.Core.Models;

public class TagValueChangedEventArgs : EventArgs
{
    public required string TagPath { get; init; }
    public required object? Value { get; init; }
    public required DateTime Timestamp { get; init; }
}