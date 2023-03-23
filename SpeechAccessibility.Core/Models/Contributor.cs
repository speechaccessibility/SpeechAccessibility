using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpeechAccessibility.Core.Models
{
    public class Contributor
    {
        public Contributor()
        {
            Consent = new HashSet<Consent>();
            ContributorDetails = new HashSet<ContributorDetails>();
        }

        public Guid Id { get; set; }
        public string UnderstandSpeechInd { get; set; }
        public string ParkinsonsInd { get; set; }
        public string EighteenOrOlderInd { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string HelperEmail { get; set; }
        public string HelperFirstName { get; set; }
        public string HelperLastName { get; set; }
        public string StateResidence { get; set; }
        public string IdentityUserId { get; set; }
        public int StatusId { get; set; }
        public DateTime CreateTS { get; set; }
        public DateTime UpdateTS { get; set; }
        public string Comments { get; set; }
        public bool? ChangePassword { get; set; }
        public bool? ContactLSVT { get; set; }
        public string PhoneNumber { get; set; }

     
        public ContributorStatus ContributorStatus { get; set; }
        public  ICollection<Consent> Consent { get; set; }
        public  ICollection<ContributorDetails> ContributorDetails { get; set; }

    }
}
