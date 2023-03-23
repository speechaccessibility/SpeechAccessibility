using System;

namespace SpeechAccessibility.Core.Models
{
    public class ContributorRace
    {
        public int Id { get; set; }
        public int? RacialGroupId { get; set; }
        public string OtherRace { get; set; }
        public int? ContributorDetailsId { get; set; }
        public DateTime CreateTS { get; set; }
        public DateTime UpdateTS { get; set; }

        public  ContributorDetails ContributorDetails { get; set; }
        public  RacialGroup RacialGroup { get; set; }
    }
}
