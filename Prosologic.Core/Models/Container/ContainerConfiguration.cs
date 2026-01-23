using Prosologic.Core.Enums.Container;

namespace Prosologic.Core.Models.Container
{
    public class ContainerConfiguration
    {
        public string Image { get; set; } = "prosologic-runtime:latest";
        public List<string> Ports { get; set; } = new();
        public List<string> Volumes { get; set; } = new();
        public Dictionary<string, string> Environment { get; set; } = new();
        public ResourceLimits? Resources { get; set; }
        public RestartPolicy RestartPolicy { get; set; } = RestartPolicy.UnlessStopped;
        public string? ContainerName { get; set; }

        public ValidationResult Validate()
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            if (string.IsNullOrWhiteSpace(Image))
                errors.Add("Container image is required");

            if (!Ports.Any())
                warnings.Add("No ports mapped - container may not be accessible");

            return new ValidationResult
            {
                IsValid = !errors.Any(),
                Errors = errors,
                Warnings = warnings
            };
        }
    }
}
