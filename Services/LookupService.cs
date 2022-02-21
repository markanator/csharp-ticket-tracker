using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Services
{
    public class LookupService : ILookupService
    {
        private readonly ApplicationDbContext _contextService;

        public LookupService(ApplicationDbContext context)
        {
            _contextService = context;
        }

        public async Task<List<ProjectPriority>> GetAllProjectPrioritiesAsync()
        {
            try
            {
                return await _contextService.ProjectPriorities.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketStatus>> GetAllTicketStatusAsync()
        {
            try
            {
                return await _contextService.TicketStatuses.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketType>> GetAllTicketTypesAsync()
        {
            try
            {
                return await _contextService.TicketTypes.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketPriority>> GetTicketPrioritiesAsync()
        {
            try
            {
                return await _contextService.TicketPriorities.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
