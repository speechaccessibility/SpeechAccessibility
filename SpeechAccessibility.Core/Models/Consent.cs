using System;

namespace SpeechAccessibility.Core.Models
{
    public class Consent
    {
        public int Id { get; set; }
        public string Version { get; set; }
        public string Name { get; set; }
        public DateTime CreateTS { get; set; }
        public DateTime UpdateTS { get; set; }
        public Guid ContributorId { get; set; }

        public  ContributorView Contributor { get; set; }
    }
}
