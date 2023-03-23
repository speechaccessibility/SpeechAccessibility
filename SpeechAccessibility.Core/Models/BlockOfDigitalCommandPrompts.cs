using System;

namespace SpeechAccessibility.Core.Models
{
    public class BlockOfDigitalCommandPrompts
    {
        public int Id { get; set; }
        public int BlockOfDigitalCommandId { get; set; }
        public int PromptId { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? CreateTS { get; set; }
        public string Active { get; set; }

        public  BlockOfDigitalCommand BlockOfDigitalCommand { get; set; }
        public  Category Category { get; set; }
        public  Prompt Prompt { get; set; }
    }
}
