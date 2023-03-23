using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
    public class ContributorStatus
    {
        public ContributorStatus()
        {
            Contributor = new HashSet<Contributor>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public  ICollection<Contributor> Contributor { get; set; }
    }
}
