using System;

namespace SpeechAccessibility.Core.Models
{
    public class BlockMasterOfPrompts
    {
        public int Id { get; set; }
        public int BlockMasterId { get; set; }
        public int PromptId { get; set; }
        public int CategoryId { get; set; }
        public DateTime CreateTS { get; set; }
        public string Active { get; set; }

        public  BlockMaster BlockMaster { get; set; }
        public  Category Category { get; set; }
        public  Prompt Prompt { get; set; }
    }
}
