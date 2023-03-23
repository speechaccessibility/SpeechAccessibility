using System;
using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
    public class BlockMaster
    {
        public BlockMaster()
        {
            BlockMasterOfPrompts = new HashSet<BlockMasterOfPrompts>();
        }

        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime? CreateTS { get; set; }
        public string Active { get; set; }

        public  ICollection<BlockMasterOfPrompts> BlockMasterOfPrompts { get; set; }
    }
}
