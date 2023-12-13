using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpeechAccessibility.Core.Models
{
    public class Prompt
    {
        public Prompt()
        {
            BlockMasterOfPrompts = new HashSet<BlockMasterOfPrompts>();
            BlockOfDigitalCommandPrompts = new HashSet<BlockOfDigitalCommandPrompts>();
            BlockOfPrompts = new HashSet<BlockOfPrompts>();
            Recording = new HashSet<Recording>();
            PromptEtiology = new HashSet<PromptEtiology>();
        }

        public int Id { get; set; }
        public string Transcript { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public string QuestionType { get; set; }
        public int? SeverityLevels { get; set; }
        public DateTime CreateTS { get; set; }
        public DateTime UpdateTS { get; set; }
        public string UpdateBy { get; set; }
        public string Active { get; set; }

        public  Category Category { get; set; }
        public  SubCategory SubCategory { get; set; }
        public  ICollection<BlockMasterOfPrompts> BlockMasterOfPrompts { get; set; }
        public  ICollection<BlockOfDigitalCommandPrompts> BlockOfDigitalCommandPrompts { get; set; }
        public  ICollection<BlockOfPrompts> BlockOfPrompts { get; set; }
        public  ICollection<Recording> Recording { get; set; }
        public ICollection<PromptEtiology> PromptEtiology { get; set; }

        [NotMapped]
        public bool InUsed { get; set; }

        [NotMapped]
        public bool CanNotDelete { get; set; }

        [NotMapped]
        public int EtioglogyId { get; set; }


    }
}
