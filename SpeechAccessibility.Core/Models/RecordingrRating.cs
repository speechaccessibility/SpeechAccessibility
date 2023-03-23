using System;

namespace SpeechAccessibility.Core.Models
{
    public  class RecordingRating 
    {
        public int Id { get; set; }
        public int RecordingId { get; set; }
        public int DimensionId { get; set; }
        public string RatingLevel { get; set; }
        public string Other { get; set; }
        public string Comment { get; set; }
        public string Active { get; set; }
        public DateTime RatingTS { get; set; }
        public string RatingBy { get; set; }
        public  Dimension Dimension { get; set; }
        public  Recording Recording { get; set; }
    }
}
