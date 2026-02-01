namespace Prosologic.Runtime.Services;

public class ScriptContext
{
    public object? value { get; set; }
    
    // Helper methods scripts can use
    public double Random(double min, double max)
    {
        return System.Random.Shared.NextDouble() * (max - min) + min;
    }
    
    public int RandomInt(int min, int max)
    {
        return System.Random.Shared.Next(min, max);
    }
}