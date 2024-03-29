﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Models.Enums;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRoleService _roleService;

        public ProjectService(ApplicationDbContext context, IRoleService roleService)
        {
            this._context = context;
            this._roleService = roleService;
        }

        // CREATE
        public async Task AddNewProjectAsync(Project project)
        {
            try
            {
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            try
            {
                var currentPM = await GetProjectManagerAsync(projectId);
                if (currentPM != null)
                {
                    try
                    {
                        await RemoveProjectManagerAsync(projectId);
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine("ERROR REMOVING PM" + ex2?.Message);
                        throw;
                    }
                }
                // add new PM
                await AddUserToProjectAsync(userId, projectId);
                return true;
            }
            catch (Exception ex1)
            {
                Console.WriteLine("ERROR ADDING PM TO PROJECT" + ex1?.Message);
                return false;
            }
        }

        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            // fetch user
            BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                // fetch project
                Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
                if (!await IsUserOnProjectAsync(user.Id, projectId))
                {
                    try
                    {
                        // attempt to add user as a member
                        project.Members.Add(user);
                        await _context.SaveChangesAsync();
                        return true;
                    }
                    catch (Exception)
                    {
                        // error when attempting to add
                        throw;
                    }
                }
                else
                {
                    // user is already on project
                    return false;
                }
            }
            // user doesn't exist
            return false;
        }

        // DELETE
        public async Task ArchiveProjectAsync(Project project)
        {
            try
            {
                project.Archived = true;
                await this.UpdateProjectAsync(project);

                // archive tickets as well
                foreach (var ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = true;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BTUser>> GetAllProjectMembersExceptPMAsync(int projectId)
        {
            List<BTUser> devs = await GetProjectMembersByRoleAsync(projectId, Roles.Developer.ToString());
            List<BTUser> reporters = await GetProjectMembersByRoleAsync(projectId, Roles.Submitter.ToString());
            List<BTUser> admins = await GetProjectMembersByRoleAsync(projectId, Roles.Admin.ToString());

            return devs.Concat(reporters).Concat(admins).ToList();
        }

        public async Task<List<Project>> GetAllProjectsByCompanyAsync(int companyId)
        {
            List<Project> projects = new();
            // TODO: don't allow archived
            projects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == false)
                                            .Include(p => p.Members)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketStatus)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketPriority)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketType)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Comments)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Attachments)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.History)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.DeveloperUser)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.OwnerUser)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Notifications)
                                            .Include(p => p.ProjectPriority)
                                            .Include(p => p.Company)
                                            .ToListAsync();
            return projects;
        }

        public async Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priorityName)
        {
            List<Project> projects = await GetAllProjectsByCompanyAsync(companyId);
            int pId = await LookupProjectPriorityIdAsync(priorityName);

            return projects.Where(p => p.ProjectPriorityId == pId).ToList();
        }

        public async Task<List<Project>> GetArchivedProjectsByCompanyAsync(int companyId)
        {
            List<Project> projects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == true)
                                            .Include(p => p.Members)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketStatus)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketPriority)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketType)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Comments)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Attachments)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.History)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.DeveloperUser)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.OwnerUser)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Notifications)
                                            .Include(p => p.ProjectPriority)
                                            .ToListAsync();

            return projects;
        }

        public async Task<List<BTUser>> GetDevelopersOnProjectAsync(int projectId)
        {
            return await GetProjectMembersByRoleAsync(projectId, Roles.Developer.ToString());
        }

        // READ BY ID
        public async Task<Project> GetProjectByIdAsync(int projectId, int companyId)
        {
            try
            {
                /*.Include(p => p.Tickets)
                    .ThenInclude(t => t.Comments)
                .Include(p => p.Tickets)
                    .ThenInclude(t => t.Attachments)
                .Include(p => p.Tickets)
                    .ThenInclude(t => t.History)*/
                return await _context.Projects.Include(p => p.Members)
                                                .Include(p => p.ProjectPriority)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketStatus)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketPriority)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketType)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.DeveloperUser)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.OwnerUser)
                                                .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            try
            {
                var project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
                foreach (var member in project?.Members)
                {
                    if (await _roleService.IsUserInRoleAsync(member, Roles.ProjectManager.ToString()))
                    {
                        return member;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

            return null;
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        {
            try
            {
                var project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
                List<BTUser> members = new();

                foreach (var member in project.Members)
                {
                    if (await _roleService.IsUserInRoleAsync(member, role))
                    {
                        members.Add(member);
                    }
                }

                return members;
            }
            catch (Exception ex)
            {
                Console.WriteLine("**** ERROR **** - Error getting by role " + ex?.Message);
                throw;
            }
        }

        public async Task<List<BTUser>> GetSubmittersOnProjectAsync(int projectId)
        {
            return await GetProjectMembersByRoleAsync(projectId, Roles.Submitter.ToString());
        }

        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            try
            {
                return (await _context.Users.Include(u => u.Projects)
                                                                .ThenInclude(p => p.Company)
                                                            .Include(u => u.Projects)
                                                                .ThenInclude(p => p.Members)
                                                            .Include(u => u.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                            .Include(u => u.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                                    .ThenInclude(t => t.DeveloperUser)
                                                            .Include(u => u.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                                    .ThenInclude(t => t.OwnerUser)
                                                            .Include(u => u.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                                    .ThenInclude(t => t.TicketPriority)
                                                            .Include(u => u.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                                    .ThenInclude(t => t.TicketStatus)
                                                            .Include(u => u.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                                    .ThenInclude(t => t.TicketType)
                                                            .FirstOrDefaultAsync(u => u.Id == userId)).Projects.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("**** ERROR ***** - Error fetching user projects" + ex?.Message);
                throw;
            }
        }

        public async Task<List<BTUser>> GetUsersNotOnProjectAsync(int projectId, int companyId)
        {
            try
            {
                // for each record, only if they are NOT attached to project
                var users = await _context.Users.Where(u => u.Projects.All(p => p.Id != projectId)).ToListAsync();
                // only for specific company
                return users.Where(u => u.CompanyId == companyId).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {
            try
            {
                Project project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);

                if (project != null)
                {
                    return project.Members.Any(m => m.Id == userId);
                }

            }
            catch (Exception)
            {

                throw;
            }
            return false;
        }

        public async Task<int> LookupProjectPriorityIdAsync(string priorityName)
        {
            try
            {
                // await thread call, and only return Id of item
                return (await _context.ProjectPriorities.FirstOrDefaultAsync(p => p.Name == priorityName)).Id;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            try
            {
                var project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
                foreach (var member in project?.Members)
                {
                    if (await _roleService.IsUserInRoleAsync(member, Roles.ProjectManager.ToString()))
                    {
                        await RemoveUserFromProjectAsync(member.Id, projectId);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
                // better error tracking
                try
                {
                    if (await IsUserOnProjectAsync(userId, projectId))
                    {
                        // we can remove
                        project.Members.Remove(user);
                        await _context.SaveChangesAsync();
                        return;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"****ERROR**** -  Error Removing user from project. ---> {ex?.Message}");
            }
        }

        public async Task RemoveUsersFromProjectByRoleAsync(string role, int projectId)
        {
            try
            {
                List<BTUser> members = await GetProjectMembersByRoleAsync(projectId, role);
                var project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
                foreach (var member in project.Members)
                {
                    try
                    {
                        project.Members.Remove(member);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("**** ERROR ***** - remove users from project" + ex?.Message);
                throw;
            }
        }

        public async Task RestoreProjectAsync(Project project)
        {
            try
            {
                project.Archived = false;
                await this.UpdateProjectAsync(project);

                // Restore tickets as well
                foreach (var ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = false;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        // UPDATE
        public async Task UpdateProjectAsync(Project project)
        {
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> IsAssignedProjectManagerAsync(string userId, int projectId)
        {
            try
            {
                string pmId = (await this.GetProjectManagerAsync(projectId))?.Id;
                return pmId == userId;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>> GetUnassignedProjectsAsync(int companyId)
        {
            var res = new List<Project>();
            try
            {
                var projects = await _context.Projects.Include(p => p.ProjectPriority).Where(p => p.CompanyId == companyId).ToListAsync();

                foreach (var project in projects)
                {
                    if ((await this.GetProjectMembersByRoleAsync(project.Id, nameof(Roles.ProjectManager))).Count == 0)
                    {
                        res.Add(project);
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
            return res;
        }
    }
}
