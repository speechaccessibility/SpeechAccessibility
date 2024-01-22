using System;
using System.Collections.Generic;

namespace SpeechAccessibility.Annotator.Models
{
    public class DailyContributorSpeechFileReportViewModel
    {
        public  DateTime TodayDate { get; set; }
        public List<int> ApprovedContributors { get; set; }  //etiologyId= 1, #Contributors = 10;  etiologyId= 2, #Contributors = 5; etiologyId= 4, #Contributors = 1; 
        public List<int> ApprovedContributorRecordings { get; set; }

        public List<int> NewContributors { get; set; }

        //public int NewContributors { get; set; }
        //public double ExistingContributors { get; set; }
        //public double NewContributorRecordings { get; set; }
        //public double ExistingContributorRecordings { get; set; }
        //public IQueryable<ContributorView> ApprovedContributors { get; set; }
        // public List<Recording> ApprovedContributorRecordings { get; set; }

        //public List<Contributor> NewContributors { get; set; }
        //public List<Guid> NewContributorIDs { get; set; }
        //public List<Tuple<Guid, int>> NewContributorWithNumberRecording { get; set; }
        //public List<Tuple<Guid, int>> ExistingContributorsNewRecordings { get; set; }
    }
}
