using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System;

namespace SpeechAccessibility.Annotator.Services
{
    public class EmailService
    {
        public async Task<string> SendEmail(string fromEmail, string[] toEmails, string[] ccEmails, string[] bccEmails, string emailSubject
            , StringBuilder emailContent, string emailServer)
        {
            var msg = new MailMessage
            {
                From = new MailAddress(fromEmail),
                IsBodyHtml = true
            };
            if (toEmails.Length < 1)
                return "To Email Address is required.";

            foreach (var toEmail in toEmails)
            {
                if (toEmail != "")
                    msg.To.Add(toEmail);
            }

            if (ccEmails != null)
            {
                foreach (var ccEmail in ccEmails)
                {
                    msg.CC.Add(ccEmail);
                }
            }

            if (bccEmails != null)
            {
                foreach (var bccEmail in bccEmails)
                {
                    msg.CC.Add(bccEmail);
                }
            }

            msg.Subject = emailSubject;
            msg.Body = emailContent.ToString();
            //var attachment = new System.Net.Mail.Attachment(fullPath);
            //msg.Attachments.Add(attachment);
            try
            {
                using var smtp = new SmtpClient(emailServer);
                await smtp.SendMailAsync(msg);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }

    }
}
