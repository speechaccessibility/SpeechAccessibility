using System;

namespace SpeechAccessibility.Core.Models
{
    public  class ContributorFollowUp
    {
        public int Id { get; set; }
        public Guid ContributorId { get; set; }
        public DateTime ScheduledSendDate { get; set; }
        public string SendToContributor { get; set; }
        public string SendToHelper { get; set; }
        public string SendToMentor { get; set; }
        public string MentorEmailAddress { get; set; }
        public string EmailContent { get; set; }
        public DateTime? CreateTS { get; set; }
        public string SendBy { get; set;}
        public DateTime? EmailSentDate { get; set; }
    }
}
