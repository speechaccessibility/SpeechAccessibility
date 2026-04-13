using System;
using System.ComponentModel.DataAnnotations;

namespace SpeechAccessibility.Data.Entities
{
    public class DeafScreening
    {
        public int Id { get; set; }

        [Required]
        public string LeftListeningDevice { get; set; }

        [Required]
        public string RightListeningDevice { get; set; }

        [Required]
        public string LanguageUse { get; set; }

        [Required]
        public Guid ContributorId { get; set; }

        public DateTime CreateTS { get; set; }
    }
}
