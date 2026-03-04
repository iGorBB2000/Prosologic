using Prosologic.Core.Enums;
using Prosologic.Core.Models;

namespace Prosologic.Runtime.Services;

public class TagContext
{
    public required Tag Tag { get; init; }
    public required string Path { get; init; }
    public object? CurrentValue { get; set; }
    public DateTime LastUpdate { get; set; }
    public Timer? UpdateTimer { get; set; }
    
    // For random strategy
    private static readonly Random _random = new();
    
    public object? GenerateRandomValue()
    {
        return Tag.DataType switch
        {
            TagDataType.Boolean => _random.Next(0, 2) == 1,
            TagDataType.Byte => (byte)_random.Next(0, 256),
            TagDataType.Int16 => (short)_random.Next(short.MinValue, short.MaxValue),
            TagDataType.Int32 => _random.Next(int.MinValue, int.MaxValue),
            TagDataType.Int64 => (long)_random.Next(int.MinValue, int.MaxValue),
            TagDataType.Float => (float)(_random.NextDouble() * 200 - 100),
            TagDataType.Double => _random.NextDouble() * 200 - 100,
            TagDataType.String => $"Random_{_random.Next(1000, 9999)}",
            TagDataType.DateTime => DateTime.Now,
            _ => null
        };
    }
}