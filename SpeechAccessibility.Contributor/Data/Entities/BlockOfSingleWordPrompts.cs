using SpeechAccessibility.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace SpeechAccessibility.Data.Entities
{
    public class BlockOfSingleWordPrompts
    {
        public int Id { get; set; }
        public int BlockOfSingleWordsId{ get; set; }  
        public Prompt Prompt { get; set; }  

        public string Active { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateTS { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateTS { get; set; }
    }

}
