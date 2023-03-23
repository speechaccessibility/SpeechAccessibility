using System;
using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
    public class Section
    {
        public Section()
        {
            Prompt = new HashSet<Prompt>();
        }
        public int Id { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string Active { get; set; }
        public DateTime CreateTs { get; set; }
        public DateTime UpdateTs { get; set; }

        public  ICollection<Prompt> Prompt { get; set; }
    }
}
