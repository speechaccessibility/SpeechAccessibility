using System;

namespace SpeechAccessibility.Core.Models
{
    public class Contributor
    {

        public Guid Id { get; set; }
        //public string UnderstandSpeechInd { get; set; }
        public int EtiologyId { get; set; }
        public string EighteenOrOlderInd { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string HelperInd { get; set; }
        public string HelperEmail { get; set; }
        public string HelperFirstName { get; set; }
        public string HelperLastName { get; set; }
        //public string StateResidence { get; set; }
        public string IdentityUserId { get; set; }
        public int StatusId { get; set; }
        public int? SubStatusId { get; set; }
        public DateTime CreateTS { get; set; }
        public DateTime? UpdateTS { get; set; }
        public string Comments { get; set; }
        public bool? ChangePassword { get; set; }
        //public bool? ContactLsvt { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? ApproveTS { get; set; }
        public string ApproveDenyBy { get; set; }
        public string OtherEtiologyText { get; set; }
        public string BirthYear { get; set; }
        public string HelperPhoneNumber { get; set; }
        public int? PromptCategoryId { get; set; }

        public Etiology Etiology { get; set; }
        public AspNetUsers IdentityUser { get; set; }

        //public  ContributorStatus ContributorStatus { get; set; }
        //public ContributorSubStatus ContributorSubStatus { get; set; }
        //public  ICollection<Consent> Consent { get; set; }
        //public  ICollection<ContributorDetails> ContributorDetails { get; set; }


    }
}
