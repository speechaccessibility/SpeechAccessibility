using System;

namespace SpeechAccessibility.Data.Entities
{
    public class FollowupPrompt
    {
        public int Id { get; set; }

        public int InitialPromptId { get; set; }

        public int FollowupPromptId { get; set; }

        public DateTime CreateTS { get; set; }

        public DateTime UpdateTS { get; set; }
    }
}
