using System;

namespace SpeechAccessibility.Core.Models
{
    public  class ContributorFollowUp
    {
        public int Id { get; set; }
        public Guid? ContributorId { get; set; }
        public string EmailContent { get; set; }
        public DateTime? SendTS { get; set; }
        public string SendBy { get; set;}
    }
}
