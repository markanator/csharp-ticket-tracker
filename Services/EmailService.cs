using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Threading.Tasks;
using TheBugTracker.Models;

namespace TheBugTracker.Services
{
    public class EmailService : IEmailSender
    {
        private readonly MailSettings _mailSettings;

        public EmailService(IOptions<MailSettings> mailSettings)
        {
            this._mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string emailTo, string subject, string htmlMessage)
        {
            MimeMessage email = new();

            email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
            email.To.Add(MailboxAddress.Parse(emailTo));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };

            email.Body = builder.ToMessageBody();


            try
            {
                // work to send email
                using var smtp = new SmtpClient();
                smtp.Connect(_mailSettings.Host, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);

                await smtp.SendAsync(email);
                smtp.Disconnect(true);
            }
            catch (System.Exception)
            {

                throw;
            }
        }
    }
}
