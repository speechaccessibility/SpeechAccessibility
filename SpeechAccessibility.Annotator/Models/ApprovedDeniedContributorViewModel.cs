using System;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Models
{
    public class ApprovedDeniedContributorViewModel
    {
        public ContributorView Contributor { get; set; }
        public  int NumberAssignBlocks { get; set; }
        public DateTime? LastRecording { get; set; }
        public  string AnnotatorAssigned { get; set; }
        public string FollowUpDate { get; set; }
        public string ApprovedStatus { get; set; }
    }
}
