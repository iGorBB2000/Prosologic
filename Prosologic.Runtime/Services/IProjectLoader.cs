using Prosologic.Core.Models;

namespace Prosologic.Runtime.Services;

public interface IProjectLoader
{
    Task<Project> LoadProjectAsync(string projectPath);
}