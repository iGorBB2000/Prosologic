using Prosologic.Core.Enums;
using Prosologic.Core.Models.Container;
using Prosologic.Core.Models.Mqtt;

namespace Prosologic.Core.Models
{
    public class Project
    {
        public string ProjectName { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0.0";
        public string? Author { get; set; }
        public DateTime Modified { get; set; }
        public string? Description { get; set; }

        public ProtocolConfiguration Protocol { get; set; } = new MqttConfiguration();
        public List<TagGroup> TagGroups { get; set; } = new();
        public ContainerConfiguration ContainerConfig { get; set; } = new();

        public IEnumerable<(TagGroup Group, Tag Tag, string Path)> GetAllTags()
        {
            foreach (var group in TagGroups)
            {
                foreach (var item in GetTagsRecursive(group, group.Name))
                {
                    yield return item;
                }
            }
        }

        private IEnumerable<(TagGroup Group, Tag Tag, string Path)> GetTagsRecursive(
            TagGroup group,
            string currentPath)
        {
            foreach (var tag in group.Tags)
            {
                yield return (group, tag, $"{currentPath}/{tag.Name}");
            }

            foreach (var subGroup in group.SubGroups)
            {
                foreach (var item in GetTagsRecursive(subGroup, $"{currentPath}/{subGroup.Name}"))
                {
                    yield return item;
                }
            }
        }

        public Tag? FindTagByPath(string path)
        {
            return GetAllTags()
                .FirstOrDefault(t => t.Path == path)
                .Tag;
        }

        public ProtocolType GetProtocolType()
        {
            return Protocol.ProtocolType;
        }

        public bool IsTagNameUniqueInGroup(TagGroup parentGroup, string tagName)
        {
            return !parentGroup.Tags.Any(t => t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsTagGroupNameUnique(TagGroup parentGroup, string groupName)
        {
            return !parentGroup.SubGroups.Any(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsTagGroupNameUniqueAtRoot(string groupName)
        {
            return !TagGroups.Any(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase));
        }

        public ValidationResult Validate()
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            if (string.IsNullOrWhiteSpace(ProjectName))
                errors.Add("Project name is required");

            if (string.IsNullOrWhiteSpace(Version))
                errors.Add("Project version is required");

            var protocolValidation = Protocol.Validate();
            if (!protocolValidation.IsValid)
            {
                errors.AddRange(protocolValidation.Errors);
            }
            warnings.AddRange(protocolValidation.Warnings);

            if (!TagGroups.Any())
            {
                warnings.Add("Project has no tag groups");
            }

            var allTags = GetAllTags().ToList();
            if (!allTags.Any())
            {
                warnings.Add("Project has no tags");
            }

            var duplicatePaths = allTags
                .GroupBy(t => t.Path)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicatePaths.Any())
            {
                errors.Add($"Duplicate tag paths found: {string.Join(", ", duplicatePaths)}");
            }

            foreach (var (group, tag, path) in allTags)
            {
                if (string.IsNullOrWhiteSpace(tag.Name))
                    errors.Add($"Tag at path '{path}' has no name");

                if (tag.UpdateStrategy == UpdateStrategy.ScriptDriven
                    && string.IsNullOrWhiteSpace(tag.ScriptPath))
                {
                    warnings.Add($"Script-driven tag '{path}' has no script path");
                }
            }

            var containerValidation = ContainerConfig.Validate();
            if (!containerValidation.IsValid)
            {
                errors.AddRange(containerValidation.Errors);
            }
            warnings.AddRange(containerValidation.Warnings);

            return new ValidationResult
            {
                IsValid = !errors.Any(),
                Errors = errors,
                Warnings = warnings
            };
        }
    }
}