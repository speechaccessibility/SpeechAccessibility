using SpeechAccessibility.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace SpeechAccessibility.Data.Entities
{
    public class BlockMasterOfPrompts
    {
        public int Id { get; set; }

        public BlockMaster BlockMaster { get; set; }    

        public Prompt Prompt { get; set; }

        public Category Category {get;set;}

        public string Active { get; set; }

   [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreateTS { get; set; }

}
}
