using System;
using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
    public class BlockOfDigitalCommand
    {
        public BlockOfDigitalCommand()
        {
            BlockOfDigitalCommandPrompts = new HashSet<BlockOfDigitalCommandPrompts>();
        }

        public int Id { get; set; }
        public int ListId { get; set; }
        public string Description { get; set; }
        public DateTime? CreateTS { get; set; }
        public DateTime? UpdateTS { get; set; }
        public string Active { get; set; }

        public  List List { get; set; }
        public  ICollection<BlockOfDigitalCommandPrompts> BlockOfDigitalCommandPrompts { get; set; }
    }
}
