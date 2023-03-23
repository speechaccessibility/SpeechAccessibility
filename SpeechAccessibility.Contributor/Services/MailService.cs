using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SpeechAccessibility.Services
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _config;

        public MailService(IConfiguration config)
            {
            _config = config;
            }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            string from = _config["AdminEmail"];
            MailMessage message = new MailMessage(from, email);
            message.Subject = subject;
            message.Body = htmlMessage;
            message.IsBodyHtml = true;
            SmtpClient client = new SmtpClient("outbound-relays.techservices.illinois.edu");
            // Credentials are necessary if the server requires the client
            // to authenticate before it will send email on the client's behalf.
            client.UseDefaultCredentials = true;
            await client.SendMailAsync(message);
        }

    }
}
