using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechAccessibility.Core.Models
{
    public class ContributorSubStatus
    {
        public ContributorSubStatus()
        {
            Contributor = new HashSet<ContributorView>();
        }

        public int Id { get; set; }
        public int StatusId { get; set; }
        public string Name { get; set; }
        public int? DisplayOrder { get; set; }
        public  ContributorStatus Status { get; set; }
        public  ICollection<ContributorView> Contributor { get; set; }
    }
}
