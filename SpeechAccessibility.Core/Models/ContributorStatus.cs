using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
    public class ContributorStatus
    {
        public ContributorStatus()
        {
            Contributor = new HashSet<ContributorView>();
            ContributorSubStatus = new HashSet<ContributorSubStatus>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public  ICollection<ContributorView> Contributor { get; set; }
        public  ICollection<ContributorSubStatus> ContributorSubStatus { get; set; }
    }
}
