using System.Collections.Generic;
using System.Threading.Tasks;
using TheBugTracker.Models;

namespace TheBugTracker.Services.Interfaces
{
    public interface ILookupService
    {
        Task<List<TicketPriority>> GetTicketPrioritiesAsync();
        Task<List<TicketStatus>> GetAllTicketStatusAsync();
        Task<List<TicketType>> GetAllTicketTypesAsync();
        Task<List<ProjectPriority>> GetAllProjectPrioritiesAsync();
    }
}
