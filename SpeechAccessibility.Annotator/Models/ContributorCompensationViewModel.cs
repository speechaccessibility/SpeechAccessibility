using System;
using System.Collections.Generic;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Models
{
    public class ContributorCompensationViewModel
    {
        //public List<Tuple<ContributorView, int>> ContributorsQualifyForFirstCard { get; set; }
        //public List<Tuple<ContributorView, int>> ContributorsQualifyForSecondCard { get; set; }
        //public List<Tuple<ContributorView, int>> ContributorsQualifyForThirdCard { get; set; }
        public List<ContributorCompensationView> ContributorsQualifyForFirstCard { get; set; }
        public List<ContributorCompensationView> ContributorsQualifyForSecondCard { get; set; }
        public List<ContributorCompensationView> ContributorsQualifyForThirdCard { get; set; }

    }
}
