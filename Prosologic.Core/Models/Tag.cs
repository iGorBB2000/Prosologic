using Prosologic.Core.Enums;

namespace Prosologic.Core.Models
{
    public class Tag
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TagDataType DataType { get; set; } = TagDataType.Float;
        public string? EngineeringUnit { get; set; }
        public object? InitialValue { get; set; }

        public UpdateStrategy UpdateStrategy { get; set; } = UpdateStrategy.ScriptDriven;
        public int UpdateInterval { get; set; } = 1000; // ms
        public string? ScriptPath { get; set; }
        public TagAccessMode AccessMode { get; set; } = TagAccessMode.ReadWrite;
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
