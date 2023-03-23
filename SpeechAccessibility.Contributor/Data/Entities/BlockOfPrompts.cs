using SpeechAccessibility.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace SpeechAccessibility.Data.Entities
{
    public class BlockOfPrompts
    {
        public int Id { get; set; }
        public Block Block { get; set; }
        public Prompt Prompt { get; set; }  
        public Category Category { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateTS { get; set; }
        public string UpdateBy { get; set; }
        public string Active { get; set; }
    } 
}
