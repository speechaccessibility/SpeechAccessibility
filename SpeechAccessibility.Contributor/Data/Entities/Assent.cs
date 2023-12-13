using System;

namespace SpeechAccessibility.Data.Entities
{
    public class Assent
    {
        public int Id { get; set; }
        public string CapableOfReadingInd { get; set; }
        public string Name { get; set; }
        public string PersonObtainingAssent { get; set; }
        public Guid ContributorId { get; set; }
        public int Version { get; set; }
        public DateTime CreateTS { get; set; }
        public DateTime UpdateTS { get; set; }
    }
}
