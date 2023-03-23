using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechAccessibility.Models
{
    public class ContactViewModel
    {
        [Required]
        public string Subject { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; }

        public string Status { get; set; }

        public string Error { get; set; }
    }
}
