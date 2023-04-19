using System;
using System.Collections.Generic;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Models
{
    public class DailyContributorSpeechFileReportViewModel
    {
        public  DateTime TodayDate { get; set; }
        public int NewContributors { get; set; }
        public double ExistingContributors { get; set; }
        public double NewContributorRecordings { get; set; }
        public double ExistingContributorRecordings { get; set; }

        //public List<Contributor> NewContributors { get; set; }
        public List<Guid> NewContributorIDs { get; set; }
        public List<Tuple<Guid, int>> NewContributorWithNumberRecording { get; set; }
        public List<Tuple<Guid, int>> ExistingContributorsNewRecordings { get; set; }
    }
}
