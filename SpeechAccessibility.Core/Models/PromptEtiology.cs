using System;

namespace SpeechAccessibility.Core.Models
{
    public class PromptEtiology
    {
        public int Id { get; set; }
        public int PromptId { get; set; }
        public int EtiologyId { get; set; }
        public DateTime CreateTS{ get; set; }
        public DateTime UpdateTS { get; set; }

        public  Prompt Prompt { get; set; }
    }
}
