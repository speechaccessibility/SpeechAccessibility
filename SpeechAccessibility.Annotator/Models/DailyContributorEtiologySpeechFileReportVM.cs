using System.Collections.Generic;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Models
{
    public class DailyContributorEtiologySpeechFileReportVM
    {

        public EtiologyView EtiologyView { get; set; }
        public List<DailyContributorSpeechFileReportViewModel> DailyContributorSpeechFileReportViewModel { get; set; }
        //public DailyContributorSpeechFileReportViewModel DailyContributorSpeechFileReportViewModel { get; set; }
    }
}
