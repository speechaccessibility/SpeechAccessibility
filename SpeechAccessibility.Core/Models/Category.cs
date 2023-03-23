using System;
using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
    public class Category
    {
        public Category()
        {
            BlockMasterOfPrompts = new HashSet<BlockMasterOfPrompts>();
            BlockOfDigitalCommandPrompts = new HashSet<BlockOfDigitalCommandPrompts>();
            BlockOfPrompts = new HashSet<BlockOfPrompts>();
            Prompt = new HashSet<Prompt>();
            SubCategory = new HashSet<SubCategory>();
        }

        public int Id { get; set; }
        public string Description { get; set; }
        public string Active { get; set; }
        public DateTime? CreateTS { get; set; }
        public DateTime? UpdateTS { get; set; }

        public  ICollection<BlockMasterOfPrompts> BlockMasterOfPrompts { get; set; }
        public  ICollection<BlockOfDigitalCommandPrompts> BlockOfDigitalCommandPrompts { get; set; }
        public  ICollection<BlockOfPrompts> BlockOfPrompts { get; set; }
        public  ICollection<Prompt> Prompt { get; set; }
        public  ICollection<SubCategory> SubCategory { get; set; }
    }
}
