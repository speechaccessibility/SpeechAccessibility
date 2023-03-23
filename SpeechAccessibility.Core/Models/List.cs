using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
    public class List
    {
        public List()
        {
            AssignedDigitalCommandBlock = new HashSet<AssignedDigitalCommandBlock>();
            BlockOfDigitalCommand = new HashSet<BlockOfDigitalCommand>();
        }

        public int Id { get; set; }
        public string Description { get; set; }

        public  ICollection<AssignedDigitalCommandBlock> AssignedDigitalCommandBlock { get; set; }
        public  ICollection<BlockOfDigitalCommand> BlockOfDigitalCommand { get; set; }
    }
}
