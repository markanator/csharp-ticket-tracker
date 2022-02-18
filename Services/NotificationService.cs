using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _contextService;
        private readonly IEmailSender _emailSenderService;
        private readonly IRoleService _roleServiceService;

        public NotificationService(ApplicationDbContext context, IEmailSender emailSender, IRoleService roleService)
        {
            _contextService = context;
            _emailSenderService = emailSender;
            _roleServiceService = roleService;
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            try
            {
                await _contextService.Notifications.AddAsync(notification);
                await _contextService.SaveChangesAsync();
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        public async Task<List<Notification>> GetReceivedNotificationsAsync(string userId)
        {
            try
            {
                var notifications = await _contextService.Notifications
                                                         .Include(n => n.Recipient)
                                                         .Include(n => n.Sender)
                                                         .Include(n => n.Ticket)
                                                             .ThenInclude(t => t.Project)
                                                         .Where(n => n.RecipientId == userId).ToListAsync();
                return notifications;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Notification>> GetSentNotificationsAsync(string userId)
        {
            try
            {
                var notifications = await _contextService.Notifications
                                                         .Include(n => n.Recipient)
                                                         .Include(n => n.Sender)
                                                         .Include(n => n.Ticket)
                                                             .ThenInclude(t => t.Project)
                                                         .Where(n => n.SenderId == userId).ToListAsync();
                return notifications;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject)
        {
            try
            {
                var user = await _contextService.Users.FirstOrDefaultAsync(u => u.Id == notification.RecipientId);
                if (user != null)
                {
                    // send email
                    try
                    {
                        await _emailSenderService.SendEmailAsync(user.Email, emailSubject, notification.Message);
                        return true;
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task SendEmailNotificationsByRoleAsync(Notification notification, int companyId, string role)
        {
            try
            {
                var members = await _roleServiceService.GetUsersInRoleAsync(role, companyId);
                foreach (var member in members)
                {
                    notification.RecipientId = member.Id;
                    await SendEmailNotificationAsync(notification, notification.Title);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task SendMembersEmailNotificationsAsync(Notification notification, List<BTUser> members)
        {
            try
            {
                foreach (var member in members)
                {
                    notification.RecipientId = member.Id;
                    await SendEmailNotificationAsync(notification, notification.Title);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
