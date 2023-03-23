using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.ComponentModel.DataAnnotations;

namespace SpeechAccessibility.Data.Entities
{
    public class RacialGroup
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string RacialGroupName { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateTS { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateTS { get; set; }
    }
}
