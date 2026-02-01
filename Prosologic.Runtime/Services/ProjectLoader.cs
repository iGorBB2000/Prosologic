using Prosologic.Core.Models;
using Prosologic.Core.Serialization;

namespace Prosologic.Runtime.Services;

public class ProjectLoader : IProjectLoader
{
    private readonly ILogger<ProjectLoader> _logger;
    private readonly ProjectSerializer _serializer;
    
    public ProjectLoader(ILogger<ProjectLoader> logger)
    {
        _logger = logger;
        _serializer = new ProjectSerializer();
    }
    
    public async Task<Project> LoadProjectAsync(string projectPath)
    {
        _logger.LogInformation("Loading project from: {ProjectPath}", projectPath);
        
        try
        {
            var project = await _serializer.LoadAsync(projectPath);
            
            _logger.LogInformation("Project loaded: {ProjectName}", project.ProjectName);
            _logger.LogInformation("Version: {Version}", project.Version);
            _logger.LogInformation("Protocol: {Protocol}", project.Protocol.GetType().Name);
            
            var tagCount = CountTags(project);
            _logger.LogInformation("Total tags: {TagCount}", tagCount);
            
            return project;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load project");
            throw;
        }
    }
    
    private int CountTags(Project project)
    {
        int count = 0;
        foreach (var group in project.TagGroups)
        {
            count += CountTagsInGroup(group);
        }
        return count;
    }
    
    private int CountTagsInGroup(TagGroup group)
    {
        int count = group.Tags.Count;
        foreach (var subGroup in group.SubGroups)
        {
            count += CountTagsInGroup(subGroup);
        }
        return count;
    }
}