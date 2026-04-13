using System;
using System.ComponentModel.DataAnnotations;

namespace SpeechAccessibility.Data.Entities
{
    public class SituationRating
    {
        public int Id { get; set; }

        [Required]
        public int StutterSituationId { get; set; }

        [Required]
        public int Rating { get; set; }

        public Guid ContributorId { get; set; }

        public DateTime CreateTS { get; set; }
    }
}
