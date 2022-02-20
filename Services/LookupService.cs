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

        public Task<List<TicketStatus>> GetAllTicketStatusAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<List<TicketType>> GetAllTicketTypesAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<List<TicketPriority>> GetTicketPrioritiesAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
