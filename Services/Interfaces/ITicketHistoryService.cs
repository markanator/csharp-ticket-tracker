using System.Collections.Generic;
using System.Threading.Tasks;
using TheBugTracker.Models;

namespace TheBugTracker.Services.Interfaces
{
    public interface ITicketHistoryService
    {
        Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId);
        Task AddHistoryAsync(int ticketId, string model, string userId);
        Task<List<TicketHistory>> GetProjectTicketsHisotriesAsync(int projectId, int companyId);
        Task<List<TicketHistory>> GetCompanyTicketsHisotriesAsync(int companyId);
    }
}
