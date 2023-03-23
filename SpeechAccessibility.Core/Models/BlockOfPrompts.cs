using System;

namespace SpeechAccessibility.Core.Models
{
    public class BlockOfPrompts 
    {
        public int Id { get; set; }
        public int BlockId { get; set; }
        public int PromptId { get; set; }
        public int CategoryId { get; set; }
        public DateTime? CreateTS { get; set; }
        public string UpdateBy { get; set; }
        public string Active { get; set; }

        public  Block Block { get; set; }
        public  Prompt Prompt { get; set; }
        public Category Categorty { get; set; }
    }
}

