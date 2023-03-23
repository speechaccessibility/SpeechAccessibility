using System;

namespace SpeechAccessibility.Core.Models
{
    public class AssignedDigitalCommandBlock
    {
        public int Id { get; set; }
        public int ListId { get; set; }
        public Guid ContributorId { get; set; }
        public DateTime CreateTs { get; set; }
        public DateTime UpdateTs { get; set; }

        public  List List { get; set; }
    }
}
