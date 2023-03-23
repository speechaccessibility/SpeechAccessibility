using System;
using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
    public class RacialGroup
    {
        public RacialGroup()
        {
            ContributorRace = new HashSet<ContributorRace>();
        }

        public int Id { get; set; }
        public string RacialGroupName { get; set; }
        public DateTime CreateTS { get; set; }
        public DateTime UpdateTS { get; set; }

        public  ICollection<ContributorRace> ContributorRace { get; set; }
    }
}

