using SpeechAccessibility.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechAccessibility.Models
{
    public class Prompt
    {
        public int Id { get; set; }
        public string Transcript { get; set; }
        public Category Category { get; set; }

        public SubCategory SubCategory { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateTS { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateTS { get; set; }
        public string UpdateBy { get; set; }  
        public string Active { get; set; }

    }
}
