using IdentityUI.Interfaces;
using Microsoft.Extensions.Configuration;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class EmailService : IEmailService
    {
        private readonly SmtpClientModel _smtpClient;

        public EmailService(IConfiguration config)
        {
            _smtpClient = config.GetSection("MailSettings").Get<SmtpClientModel>();
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MailMessage();

            if (string.IsNullOrEmpty(email))
            {
                throw new InvalidDataException("email address is required");
            }

            message.To.Add(new MailAddress(email));
            message.From = new MailAddress(_smtpClient.From);
            message.Subject = subject;
            message.Body = htmlMessage;
            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                smtp.Credentials = new System.Net.NetworkCredential(_smtpClient.UserName, _smtpClient.Password);
                smtp.Host = _smtpClient.Host;
                await smtp.SendMailAsync(message);
            }
        }

    }
}
