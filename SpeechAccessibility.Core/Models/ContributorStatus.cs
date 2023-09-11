using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
    public class ContributorStatus
    {
        public ContributorStatus()
        {
            Contributor = new HashSet<Contributor>();
            ContributorSubStatus = new HashSet<ContributorSubStatus>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public  ICollection<Contributor> Contributor { get; set; }
        public  ICollection<ContributorSubStatus> ContributorSubStatus { get; set; }
    }
}
