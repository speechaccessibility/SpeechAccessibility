using System;

namespace SpeechAccessibility.Core.Models
{
    public class ContributorAssignedAnnotator
    {
        public int Id { get; set; }
        public Guid ContributorId { get; set; }
        public int UserId { get; set; }
        public  User User { get; set; }
    }
}
