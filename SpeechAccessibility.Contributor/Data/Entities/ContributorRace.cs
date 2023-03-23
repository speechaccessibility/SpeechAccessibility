using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace SpeechAccessibility.Data.Entities
{
    public class ContributorRace
    {
        public int Id { get; set; }
        public RacialGroup RacialGroup { get; set; }
        public string OtherRace { get; set; }
        public ContributorDetails ContributorDetails { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateTS { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateTS { get; set; }
    }
}
