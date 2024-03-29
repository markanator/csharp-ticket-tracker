﻿using System.Collections.Generic;
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
        Task<List<Project>> GetAllProjectsByCompanyAsync(int companyId);
        Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priorityName);
        Task<List<BTUser>> GetAllProjectMembersExceptPMAsync(int projectId);
        Task<List<Project>> GetArchivedProjectsByCompanyAsync(int companyId);
        Task<List<BTUser>> GetDevelopersOnProjectAsync(int projectId);
        Task<BTUser> GetProjectManagerAsync(int projectId);
        Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role);
        Task<Project> GetProjectByIdAsync(int projectId, int companyId);
        Task<List<BTUser>> GetSubmittersOnProjectAsync(int projectId);
        Task<List<BTUser>> GetUsersNotOnProjectAsync(int projectId, int companyId);
        Task<List<Project>> GetUserProjectsAsync(string userId);
        Task<List<Project>> GetUnassignedProjectsAsync(int companyId);
        Task<bool> IsUserOnProjectAsync(string userId, int projectId);
        Task<int> LookupProjectPriorityIdAsync(string priorityName);
        Task RemoveProjectManagerAsync(int projectId);
        Task RemoveUsersFromProjectByRoleAsync(string role, int projectId);
        Task RemoveUserFromProjectAsync(string userId, int projectId);
        Task UpdateProjectAsync(Project project);
        Task RestoreProjectAsync(Project project);
        Task<bool> IsAssignedProjectManagerAsync(string userId, int projectId);
    }
}
