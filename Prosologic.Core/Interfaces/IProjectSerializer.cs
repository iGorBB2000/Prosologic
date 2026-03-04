using Prosologic.Core.Models;

namespace Prosologic.Core.Interfaces
{
    public interface IProjectSerializer
    {
        Task<Project> LoadAsync(string projectDirectory);
        Task SaveAsync(Project project, string projectDirectory);
    }
}
