using System.Collections.Generic;
using System;

namespace SpeechAccessibility.Core.Models
{
    public class DimensionCategory
    {
        public DimensionCategory()
        {
            Dimension = new HashSet<Dimension>();
        }

        public int Id { get; set; }
        public string Description { get; set; }
        public string Active { get; set; }
        public DateTime? CreateTS { get; set; }
        public int DisplayOrder { get; set; }

        public  ICollection<Dimension> Dimension { get; set; }
    }
}
