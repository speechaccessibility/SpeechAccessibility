using System;
using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
    public class SubCategory
    {
        public SubCategory()
        {
            Prompt = new HashSet<Prompt>();
        }

        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Description { get; set; }
        public DateTime? CreateTS { get; set; }
        public DateTime? UpdateTS { get; set; }
        public string Active { get; set; }
        public  Category Category { get; set; }
        public  ICollection<Prompt> Prompt { get; set; }
    }
}
