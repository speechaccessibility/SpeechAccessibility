using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Models
{
    public class RecruitingRecordingProgressViewModel
    {
        public List<EtiologyView> Etiologies { set; get; }
        public List<ContributorStatus> ContributorStatus { set; get; }
        public List<ContributorView> Contributors { set; get; }
        
    }
}
