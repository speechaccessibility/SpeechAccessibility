using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechAccessibility.Data.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ADGroupName { get; set; }
        public string Description { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateTS { get; set; }
        public string InUsed { get; set; }
    }
}
