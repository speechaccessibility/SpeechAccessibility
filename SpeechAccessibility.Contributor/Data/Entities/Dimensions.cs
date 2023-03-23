using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechAccessibility.Data.Entities
{
   
    public class Dimensions
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Diagnois { get; set; }

        [MaxLength(50)]
        public string SpeechAttribute { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateTS { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateTS { get; set; }
    }
}
