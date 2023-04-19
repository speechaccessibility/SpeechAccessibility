using System;
using System.Collections.Generic;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Models
{
    public class ContributorCompensationViewModel
    {
        public List<Tuple<Contributor, int>> ContributorsQualifyForFirstCard { get; set; }
        public List<Tuple<Contributor, int>> ContributorsQualifyForSecondCard { get; set; }
        public List<Tuple<Contributor, int>> ContributorsQualifyForThirdCard { get; set; }

    }
}
