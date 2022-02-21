using Microsoft.EntityFrameworkCore;
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
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRoleService _roleService;
        private readonly IProjectService _projectService;

        public TicketService(ApplicationDbContext context, IRoleService roleService, IProjectService projectService)
        {
            this._context = context;
            this._roleService = roleService;
            this._projectService = projectService;
        }

        // create new ticket
        public async Task AddNewTicketAsync(Ticket ticket)
        {
            try
            {
                await _context.AddAsync(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        // faux Delete
        public async Task ArchiveTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = true;
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AssignTicketAsync(int ticketId, string userId)
        {
            try
            {
                var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
                if (ticket != null)
                {
                    try
                    {
                        // EF tracks and connects theses two
                        // ability to override
                        ticket.DeveloperUserId = userId;
                        // revisit when assigning tickets
                        ticket.TicketStatusId = (await LookupTicketStatusIdAsync("Development")).Value;
                        // EF will save and build connections
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId)
        {
            try
            {
                return await _context.Projects
                                            .Where(p => p.CompanyId == companyId)
                                            .SelectMany(p => p.Tickets) // get all tickets
                                                .Include(t => t.Attachments)
                                                .Include(t => t.Comments)
                                                .Include(t => t.History)
                                                .Include(t => t.DeveloperUser)
                                                .Include(t => t.OwnerUser)
                                                .Include(t => t.TicketPriority)
                                                .Include(t => t.TicketStatus)
                                                .Include(t => t.TicketType)
                                                .Include(t => t.Project)
                                            .ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName)
        {
            try
            {
                // get valid result of lookUpCall
                var priorityId = (await LookupTicketPriorityIdAsync(priorityName)).Value;

                return await _context.Projects
                                            .Where(p => p.CompanyId == companyId)
                                            .SelectMany(p => p.Tickets) // get all tickets
                                                .Include(t => t.Attachments)
                                                .Include(t => t.Comments)
                                                .Include(t => t.History)
                                                .Include(t => t.DeveloperUser)
                                                .Include(t => t.OwnerUser)
                                                .Include(t => t.TicketPriority)
                                                .Include(t => t.TicketStatus)
                                                .Include(t => t.TicketType)
                                                .Include(t => t.Project)
                                            .Where(t => t.TicketPriorityId == priorityId)
                                            .ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName)
        {
            try
            {
                var statusId = (await LookupTicketStatusIdAsync(statusName)).Value;
                return await _context.Projects
                                            .Where(p => p.CompanyId == companyId)
                                            .SelectMany(p => p.Tickets) // get all tickets
                                                .Include(t => t.Attachments)
                                                .Include(t => t.Comments)
                                                .Include(t => t.History)
                                                .Include(t => t.DeveloperUser)
                                                .Include(t => t.OwnerUser)
                                                .Include(t => t.TicketPriority)
                                                .Include(t => t.TicketStatus)
                                                .Include(t => t.TicketType)
                                                .Include(t => t.Project)
                                            .Where(t => t.TicketStatusId == statusId)
                                            .ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName)
        {
            try
            {
                var typeId = (await LookupTicketTypeIdAsync(typeName)).Value;
                return await _context.Projects
                                            .Where(p => p.CompanyId == companyId)
                                            .SelectMany(p => p.Tickets) // get all tickets
                                                .Include(t => t.Attachments)
                                                .Include(t => t.Comments)
                                                .Include(t => t.History)
                                                .Include(t => t.DeveloperUser)
                                                .Include(t => t.OwnerUser)
                                                .Include(t => t.TicketPriority)
                                                .Include(t => t.TicketStatus)
                                                .Include(t => t.TicketType)
                                                .Include(t => t.Project)
                                            .Where(t => t.TicketTypeId == typeId)
                                            .ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            try
            {
                return (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.Archived == true).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName, int companyId, int projectId)
        {
            try
            {
                return (await GetAllTicketsByPriorityAsync(companyId, priorityName)).Where(t => t.ProjectId == projectId).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId, int companyId)
        {
            try
            {
                return (await GetTicketsByRoleAsync(role, userId, companyId)).Where(t => t.ProjectId == projectId).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByStatusAsync(string statusName, int companyId, int projectId)
        {
            try
            {
                return (await GetAllTicketsByStatusAsync(companyId, statusName)).Where(t => t.ProjectId == projectId).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId)
        {
            try
            {
                return (await GetAllTicketsByTypeAsync(companyId, typeName)).Where(t => t.ProjectId == projectId).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }
        // read by id
        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            try
            {
                return await _context.Tickets
                    .Include(t => t.DeveloperUser)
                    .Include(t => t.OwnerUser)
                    .Include(t => t.Project)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.TicketType)
                    .FirstOrDefaultAsync(m => m.Id == ticketId);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BTUser> GetTicketDeveloperAsync(int ticketId, int companyId)
        {
            var dev = new BTUser();

            try
            {
                var ticket = (await GetAllTicketsByCompanyAsync(companyId)).FirstOrDefault(t => t.Id == ticketId);
                if (ticket?.DeveloperUserId != null)
                {
                    // ticket has a dev
                    return ticket.DeveloperUser;
                }

                // nothing found
                return dev;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByRoleAsync(string role, string userId, int companyId)
        {
            var tickets = new List<Ticket>();
            try
            {
                if (role == Roles.Admin.ToString())
                {
                    tickets = await GetAllTicketsByCompanyAsync(companyId);
                }
                else if (role == Roles.Developer.ToString())
                {
                    tickets = (await GetAllTicketsByCompanyAsync(companyId))
                                    .Where(t => t.DeveloperUserId == userId).ToList();
                }
                else if (role == Roles.Submitter.ToString())
                {
                    // submitter becomes owner, they can only Create tickets
                    tickets = (await GetAllTicketsByCompanyAsync(companyId))
                                    .Where(t => t.OwnerUserId == userId).ToList();
                }
                else if (role == Roles.ProjectManager.ToString())
                {
                    // only get tickets for a specific PM
                    tickets = await GetTicketsByUserIdAsync(userId, companyId);
                }

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId)
        {
            var ticketList = new List<Ticket>();

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (await _roleService.IsUserInRoleAsync(user, Roles.Admin.ToString()))
                {
                    ticketList = (await _projectService.GetAllProjectsByCompany(companyId))
                                                        .SelectMany(p => p.Tickets).ToList();
                }
                else if (await _roleService.IsUserInRoleAsync(user, Roles.Developer.ToString()))
                {
                    ticketList = (await _projectService.GetAllProjectsByCompany(companyId))
                                                        .SelectMany(p => p.Tickets)
                                                        .Where(t => t.DeveloperUserId == userId)
                                                        .ToList();
                }
                else if (await _roleService.IsUserInRoleAsync(user, Roles.Submitter.ToString()))
                {
                    ticketList = (await _projectService.GetAllProjectsByCompany(companyId))
                                                        .SelectMany(p => p.Tickets)
                                                        .Where(t => t.OwnerUserId == userId)
                                                        .ToList();
                }
                else if (await _roleService.IsUserInRoleAsync(user, Roles.Submitter.ToString()))
                {
                    ticketList = (await _projectService.GetUserProjectsAsync(userId)).SelectMany(t => t.Tickets).ToList();
                }

                return ticketList;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int?> LookupTicketPriorityIdAsync(string priorityName)
        {
            try
            {
                return (await _context.TicketPriorities.FirstOrDefaultAsync(p => p.Name == priorityName))?.Id;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int?> LookupTicketStatusIdAsync(string statusName)
        {
            try
            {
                return (await _context.TicketStatuses.FirstOrDefaultAsync(p => p.Name == statusName))?.Id;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int?> LookupTicketTypeIdAsync(string typeName)
        {
            try
            {
                return (await _context.TicketTypes.FirstOrDefaultAsync(p => p.Name == typeName))?.Id;
            }
            catch (Exception)
            {

                throw;
            }
        }

        // update ticket
        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
