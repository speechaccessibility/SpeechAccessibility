using System;
using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
   public class Block 
    {
        public Block()
        {
            BlockOfPrompts = new HashSet<BlockOfPrompts>();
            ContributorAssignedBlock = new HashSet<ContributorAssignedBlock>();
            Recording = new HashSet<Recording>();
        }

        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime? CreateTS { get; set; }
        public DateTime? UpdateTS { get; set; }
        public string UpdateBy { get; set; }
        public string InUsed { get; set; }
        public string Active { get; set; }

        public  ICollection<BlockOfPrompts> BlockOfPrompts { get; set; }
        public  ICollection<ContributorAssignedBlock> ContributorAssignedBlock { get; set; }
        public  ICollection<Recording> Recording { get; set; }


    }
}
