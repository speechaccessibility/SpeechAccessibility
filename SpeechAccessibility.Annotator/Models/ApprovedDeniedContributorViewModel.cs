using System;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Models
{
    public class ApprovedDeniedContributorViewModel
    {
        public Contributor Contributor { get; set; }
        public  int NumberAssignBlocks { get; set; }
        public string LastRecording { get; set; }
    }
}
