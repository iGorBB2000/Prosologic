namespace Prosologic.Core.Models
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        public static ValidationResult Success() => new() { IsValid = true };

        public static ValidationResult Failure(params string[] errors) =>
            new() { IsValid = false, Errors = errors.ToList() };

        public static ValidationResult Warning(params string[] warnings) =>
            new() { IsValid = true, Warnings = warnings.ToList() };
    }
}
