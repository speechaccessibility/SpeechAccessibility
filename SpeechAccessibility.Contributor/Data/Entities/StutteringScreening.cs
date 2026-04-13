using System;
using System.ComponentModel.DataAnnotations;

namespace SpeechAccessibility.Data.Entities
{
    public class StutteringScreening
    {
        public int Id { get; set; }

        public Guid ContributorId { get; set; }

        [Required]
        public string Severity { get; set; }

        [Required]
        public int PhysicalTension { get; set; }
        [Required]
        public int NegativeThoughts { get; set; }
        [Required]
        public int AvoidStutter { get; set; }

        public DateTime CreateTS { get; set; }
    }
}
