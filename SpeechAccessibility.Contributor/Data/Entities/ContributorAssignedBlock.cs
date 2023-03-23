using SpeechAccessibility.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace SpeechAccessibility.Data.Entities
{
    public class ContributorAssignedBlock
    {
        public int Id { get; set; }
        public Guid ContributorId { get; set; }
        public Block Block { get; set; }      
        public string InUsed { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateTS { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreateTS { get; set; }
        public string UpdateBy { get; set; }    
        public string Active { get; set; }  
    }
}
