using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using SendScheduledFollowUpEmails.DAL;
using System.Text;


// See https://aka.ms/new-console-template for more information
var builder = new ConfigurationBuilder()
    .AddJsonFile($"appsetings.json", false, true);

var config = builder.Build();

var followUpEmailDAL = new FollowUpEmailDAL(config);

var scheduledEmails = followUpEmailDAL.GetScheduledFollowUpEmails();
scheduledEmails.ForEach(email =>
{
    string sendTo = "";
    string subject = "Speech Accessibility Project - Follow-up";

    if (email.ScheduledSendDate.CompareTo(DateTime.Now.Date) <= 0)
    {
        if (config["AppSettings:TestingMode"] == "Yes")
        {
            if (email.SendToMentor == "Yes")
            {
                sendTo += email.MentorEmailAddress;
            }
            else
            {
                sendTo += email.SendBy + "@illinois.edu";
            }
            subject = "Testing: " + subject;
            email.EmailContent = "This email was sent in a testing mode.The actual email should be sent to <br/>" + email.EmailContent;
        }
        else
        {
            if (email.SendToContributor == "Yes")
            {
                sendTo += email.EmailAddress;
            }

            if (email.SendToHelper == "Yes")
            {
                if (sendTo != "")
                    sendTo = sendTo + "," + email.HelperEmail;
                else
                    sendTo += email.HelperEmail;
            }
            if (email.SendToMentor == "Yes")
            {
                if (sendTo != "")
                    sendTo = sendTo + "," + email.MentorEmailAddress;
                else
                    sendTo += email.MentorEmailAddress;
            }
        }
       
        var error = SendEmail(sendTo, subject, email.EmailContent);
        if (!string.IsNullOrEmpty(error))
            Console.Write(error);
        else
        {
            followUpEmailDAL.MarkMessageSent(email.Id);
            followUpEmailDAL.SaveEmailLogging(email.ContributorId,subject,email.EmailContent,sendTo,email.SendBy);
        }
    }

});
Environment.Exit(0);


string SendEmail(string toEmails, string subject, string body) {
    
    StringBuilder followUpMessage = new StringBuilder();
    MailMessage mailMessage = new MailMessage();
    mailMessage.From = new MailAddress(config["AppSettings:SpeechAccessibilityTeamEmail"]);
    mailMessage.IsBodyHtml = true;

  
    if (config["AppSettings:DeveloperMode"] == "Yes")
    {
        mailMessage.To.Add(config["AppSettings:TestingEmail"]);
        subject = "Testing: " + subject;
        followUpMessage.Append("This email was sent in a testing mode.The actual email should be sent to '" + toEmails + "'.<br>");
    }
    else
    {
        foreach (string email in toEmails.Split(','))
        {
            if (email != "")
                mailMessage.To.Add(email);
        }
    }

    followUpMessage.Append("<br>" + body);
    mailMessage.Subject = subject;
    mailMessage.Body = followUpMessage.ToString();
    SmtpClient smtpClient = new SmtpClient();
    smtpClient.Host = config["AppSettings:EmailServer"];
    try
    {
        smtpClient.Send(mailMessage);
    }
    catch (Exception ex)
    {
        return ex.Message;
    }
  
    return "";
}

//string SendTestingEmail()
//{
//    MailMessage mailMessage = new MailMessage();
//    mailMessage.From = new MailAddress(config["AppSettings:SpeechAccessibilityTeamEmail"]);
//    mailMessage.To.Add(config["AppSettings:TestingEmail"]);
//    mailMessage.Subject = "Subject";
//    mailMessage.Body = "This is test email";

//    SmtpClient smtpClient = new SmtpClient();
//    smtpClient.Host = config["AppSettings:EmailServer"];
//    //smtpClient.Port = 587;
//    //smtpClient.UseDefaultCredentials = false;
//    //smtpClient.Credentials = new NetworkCredential("username", "password");
//    //smtpClient.EnableSsl = true;

//    try
//    {
//        smtpClient.Send(mailMessage);
//        Console.WriteLine("Email Sent Successfully.");
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine("Error: " + ex.Message);
//    }

//    return "";
//}