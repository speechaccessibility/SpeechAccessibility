using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechAccessibility.Data.Entities
{
    public class Block
    {
        public int Id { get; set; }

        public string Description { get; set; } 

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateTS { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateTS { get; set; }

        public string UpdateBy { get; set; }

        public string InUsed { get; set; }

        public string Active { get; set; }
    }
}
