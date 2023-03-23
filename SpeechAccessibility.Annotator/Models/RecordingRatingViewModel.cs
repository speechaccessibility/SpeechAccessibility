using SpeechAccessibility.Core.Models;
using System.Collections.Generic;

namespace SpeechAccessibility.Annotator.Models
{
    public class RecordingRatingViewModel
    {
        
        public Recording Recording { get; set; }
       
        public List<RecordingRating> RecordingRatingList { get; set; }
        public List<DimensionCategory> DimensionCategoryList { get; set; }
       

    }
}
