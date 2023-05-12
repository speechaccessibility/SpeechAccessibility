using System;
using System.Security;

namespace SpeechAccessibility.Core.Models
{
    public class EmailLogging
    {
        public int Id { get; set; }
        public Guid? ContributorId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime SendTS { get; set; }
        public string SendBy { get; set; }
        public string Error { get; set; } 
        public string SendTo { get; set; }
        public string SendBCC { get; set; }
        public string SendCC { get; set; }
       

    }
}
