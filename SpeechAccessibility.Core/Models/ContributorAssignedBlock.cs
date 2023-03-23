using System;

namespace SpeechAccessibility.Core.Models
{
  public  class ContributorAssignedBlock 
    {
        public int Id { get; set; }
        public Guid ContributorId { get; set; }
        public int BlockId { get; set; }
        public  string InUsed { get; set; }
        public DateTime? UpdateTS { get; set; }
        public string UpdateBy { get; set; }
        public string Active { get; set; }

        public  Block Block { get; set; }
    }
}
