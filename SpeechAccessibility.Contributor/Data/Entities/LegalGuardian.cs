using SpeechAccessibility.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpeechAccessibility.Data.Entities
{
    public class LegalGuardian
    {
        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        public Guid ContributorId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateTS { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateTS { get; set; }
    }
}
