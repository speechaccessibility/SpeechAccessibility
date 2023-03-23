using System;
using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
    public class Dimension
    {
        public Dimension()
        {
            RecordingRating = new HashSet<RecordingRating>();
        }

        public int Id { get; set; }
        public int DimensionCategoryId { get; set; }
        public string Description { get; set; }
        public DateTime? CreateTS { get; set; }
        public DateTime? UpdateTS { get; set; }
        public string Active { get; set; }

        public  DimensionCategory DimensionCategory { get; set; }
        public  ICollection<RecordingRating> RecordingRating { get; set; }

    }
}
