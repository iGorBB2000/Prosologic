namespace Prosologic.Core.Models
{
    public class TagGroup
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<Tag> Tags { get; set; } = new();
        public List<TagGroup> SubGroups { get; set; } = new();
    }
}
