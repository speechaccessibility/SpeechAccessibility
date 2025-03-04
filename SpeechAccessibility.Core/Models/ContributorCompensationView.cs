using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpeechAccessibility.Core.Models
{
    public  class ContributorCompensationView
    {
        public Guid ContributorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string HelperInd { get; set; }
        public string HelperEmail { get; set; }
        public string HelperFirstName { get; set; }
        public string HelperLastName { get; set; }
        public DateTime? SendFirstCard { get; set; }
        public DateTime? SendSecondCard { get; set; }
        public DateTime? SendThirdCard { get; set; }
        public int FirstGiftCard { get; set; }
        public int SecondGiftCard { get; set; }
        public int ThirdGiftCard { get; set; }
        public int? RecordingCount { get; set; }
        public string Etiology { get; set; }
        public string PromptCategory { get; set; }
        public string FirstCard { get; set; }
        public string SecondCard { get; set; }
        public string ThirdCard { get; set; }
        public string PaymentType { get; set; } 

        [NotMapped]
        public DateTime FirstRecordingDate { get; set; }
        [NotMapped]
        public DateTime LastRecordingDate { get; set; }

    }
}
