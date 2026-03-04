using Prosologic.Core.Interfaces;
using Prosologic.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Prosologic.Core.Serialization
{
    public class ProjectSerializer : IProjectSerializer
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public ProjectSerializer()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                    new ProtocolConfigurationConverter()
                }
            };
        }

        public async Task<Project> LoadAsync(string projectDirectory)
        {
            if (!Directory.Exists(projectDirectory))
            {
                throw new DirectoryNotFoundException($"Project directory not found: {projectDirectory}");
            }

            var projectFile = Path.Combine(projectDirectory, "project.json");
            if (!File.Exists(projectFile))
            {
                throw new FileNotFoundException("project.json not found", projectFile);
            }

            var json = await File.ReadAllTextAsync(projectFile);
            var project = JsonSerializer.Deserialize<Project>(json, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize project");

            await LoadScriptsAsync(project, projectDirectory);

            return project;
        }

        public async Task SaveAsync(Project project, string projectDirectory)
        {
            Directory.CreateDirectory(projectDirectory);
            var scriptsDir = Path.Combine(projectDirectory, "Scripts");
            Directory.CreateDirectory(scriptsDir);

            await SaveScriptsAsync(project, projectDirectory);

            project.Modified = DateTime.Now;

            var json = JsonSerializer.Serialize(project, _jsonOptions);
            var projectFile = Path.Combine(projectDirectory, "project.json");

            await File.WriteAllTextAsync(projectFile, json);
        }

        private async Task LoadScriptsAsync(Project project, string projectDirectory)
        {
            foreach (var (group, tag, path) in project.GetAllTags())
            {
                if (string.IsNullOrEmpty(tag.ScriptPath))
                    continue;

                var scriptFile = Path.Combine(projectDirectory, tag.ScriptPath);

                if (File.Exists(scriptFile))
                {
                    var scriptContent = await File.ReadAllTextAsync(scriptFile);

                    tag.Metadata["ScriptContent"] = scriptContent;
                }
            }
        }

        private async Task SaveScriptsAsync(Project project, string projectDirectory)
        {
            foreach (var (group, tag, path) in project.GetAllTags())
            {
                if (string.IsNullOrEmpty(tag.ScriptPath))
                    continue;

                if (!tag.Metadata.TryGetValue("ScriptContent", out var scriptContent))
                    continue;

                if (!tag.ScriptPath.StartsWith("Scripts/"))
                {
                    tag.ScriptPath = $"Scripts/{tag.ScriptPath}";
                }

                var scriptFile = Path.Combine(projectDirectory, tag.ScriptPath);

                var scriptDir = Path.GetDirectoryName(scriptFile);
                if (!string.IsNullOrEmpty(scriptDir))
                {
                    Directory.CreateDirectory(scriptDir);
                }

                await File.WriteAllTextAsync(scriptFile, scriptContent);
            }
        }
    }
}
