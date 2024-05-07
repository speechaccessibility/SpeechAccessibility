using System;

namespace SpeechAccessibility.Annotator.Models
{
    public class RecordingStatusViewModel
    {
        public DateTime ReportDate { get; set; }
        public Guid ContributorId { get; set; }
        public string Status { get; set; }
        public int NumberOfRecord { get; set; }
        public string EtiologyName { get; set;}
    }
}