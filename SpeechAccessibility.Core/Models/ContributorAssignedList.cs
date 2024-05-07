using System;

namespace SpeechAccessibility.Core.Models
{
    public class ContributorAssignedList
    {
        public Guid ContributorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int ListId { get; set; }
        public string BlockName { get; set; }
        public string ListName { get; set; }
    }
}
