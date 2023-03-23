using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechAccessibility.Models
{
    public class RecordPromptModel
    {
        [BindProperty]
        public InputModel Input { get; set; }
        public Prompt prompt { get; set; }
        public Guid contributorId { get; set; }
        public string status { get; set; }
        public int count { get; set; }
        public int assessmentMax { get; set; }
        public int digitalCommandMax { get; set; }
        public int novelSentenceMax { get; set; }
        public int congratulationsCount { get; set; }
        public int assessmentCount { get; set; }
        public int currentBlockOfPromptsCount { get; set; }
        public int blockId { get; set; }
        public int blockMax { get; set; }
        public int totalBlockCount { get; set; }
        public int retryCount { get; set; }
        public int phonationPromptCount { get; set; }
        public List<Prompt> spontaneousSpeechList { get; set; }

        public class InputModel
        {
            public int promptId { get; set; }
        }
    }
}
