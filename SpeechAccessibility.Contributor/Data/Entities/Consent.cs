using SpeechAccessibility.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechAccessibility.Data.Entities
{
    public class Consent
    {
        public int Id { get; set; }
        [MaxLength(3)]
        [Required]
        public string Version { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateTS { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateTS { get; set; }
        [Required]
        public Contributor Contributor { get; set; }

    }
}
