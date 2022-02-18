using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Services
{
    public class InviteService : IInviteService
    {
        private readonly ApplicationDbContext _contextService;

        public InviteService(ApplicationDbContext context)
        {
            _contextService = context;
        }

        public async Task<bool> AcceptInviteAsync(Guid? token, string userId, int companyId)
        {
            try
            {
                var invite = await _contextService.Invites.FirstOrDefaultAsync(i => i.CompanyToken == token);

                if (invite == null) { return false; }
                // record keeping info
                invite.IsValid = false;
                invite.InviteeId = userId;
                await _contextService.SaveChangesAsync();
                return true;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddNewInviteAsync(Invite invite)
        {
            try
            {
                await _contextService.Invites.AddAsync(invite);
                await _contextService.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AnyInviteAsync(Guid token, string email, int companyId)
        {
            try
            {
                return await _contextService.Invites.Where(i => i.CompanyId == companyId)
                                                    .AnyAsync(i => i.CompanyToken == token && i.InviteeEmail == email);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Invite> GetInviteAsync(int inviteId, int companyId)
        {
            try
            {
                return await _contextService.Invites.Where(i => i.CompanyId == companyId)
                                                    .Include(i => i.Company)
                                                    .Include(i => i.Project)
                                                    .Include(i => i.Invitor)
                                                    .FirstOrDefaultAsync(i => i.Id == companyId);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Invite> GetInviteAsync(Guid? token, string email, int companyId)
        {
            try
            {
                return await _contextService.Invites.Where(i => i.CompanyId == companyId)
                                                    .Include(i => i.Company)
                                                    .Include(i => i.Project)
                                                    .Include(i => i.Invitor)
                                                    .FirstOrDefaultAsync(i => i.CompanyToken == token && i.InviteeEmail == email);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> ValidateInviteCodeAsync(Guid? token)
        {
            try
            {
                if (token == null) { return false; }

                var res = false;
                var invite = await _contextService.Invites.FirstOrDefaultAsync(i => i.CompanyToken == token);

                if (invite != null)
                {
                    // determine invite date
                    DateTime inviteDate = invite.InviteDate.DateTime;

                    // custom validation of the invite based on the day it was sent, only valid for 7 days
                    bool validDate = (DateTime.Now - inviteDate).TotalDays <= 7;

                    if (validDate)
                    {
                        res = invite.IsValid;
                    }
                }

                return res;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
