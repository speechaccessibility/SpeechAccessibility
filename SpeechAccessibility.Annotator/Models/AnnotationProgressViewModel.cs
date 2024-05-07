using System;
using System.Collections.Generic;

namespace SpeechAccessibility.Annotator.Models
{
    public class AnnotationProgressViewModel
    {
        public DateTime ReportDate { get; set; }
        //public  User Annotator { get; set; }
        public int AnnotatorId { get; set; }
        //public List<Recording> AnnotatorAssignedRecordings { get; set; } 
        public List<RecordingStatusViewModel> AssignedRecordingByContributor { get; set; }
    }
}
