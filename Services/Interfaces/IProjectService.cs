using System.Collections.Generic;
using System.Threading.Tasks;
using TheBugTracker.Models;

namespace TheBugTracker.Services.Interfaces
{
    public interface IProjectService
    {
        Task AddNewProjectAsync(Project project);
        Task<bool> AddProjectManagerAsync(string userId, int projectId);
        Task<bool> AddUserToProjectAsync(string userId, int projectId);
        Task ArchiveProjectAsync(Project project);
        Task<List<Project>> GetAllProjectsByCompany(int companyId);
        Task<List<Project>> GetAllProjectsByPriority(int companyId, string priorityName);
        Task<List<BTUser>> GetAllProjectMembersExceptPMAsync(int projectId);
        Task<List<Project>> GetArchivedProjectsByCompany(int companyId);
        Task<List<BTUser>> GetDevelopersOnProjectAsync(int projectId);
        Task<BTUser> GetProjectManagerAsync(int projectId);
        Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role);
        Task<Project> GetProjectByIdAsync(int projectId, int companyId);
        Task<List<BTUser>> GetSubmittersOnProjectAsync(int projectId);
        Task<List<BTUser>> GetUsersNotOnProjectAsync(int projectId, int companyId);
        Task<List<Project>> GetUserProjectsAsync(string userId);
        Task<bool> IsUserOnProjectAsync(string userId, int projectId);
        Task<int> LookupProjectPriorityId(string priorityName);
        Task RemoveProjectManagerAsync(int projectId);
        Task RemoveUsersFromProjectByRoleAsync(string role, int projectId);
        Task RemoveUserFromProjectAsync(string userId, int projectId);
        Task UpdateProjectAsync(Project project);
        Task RestoreProjectAsync(Project project);
        Task<bool> IsAssignedProjectManagerAsync(string userId, int projectId);
    }
}
