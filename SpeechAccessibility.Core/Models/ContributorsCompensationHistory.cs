using System;

namespace SpeechAccessibility.Core.Models
{
    public class ContributorCompensationHistory
    {
        public int Id { get; set; }
        public Guid ContributorId { get; set; }
        public DateTime? SendFirstCard { get; set; }
        public string SendFirstCardBy { get; set; }
        public DateTime? SendSecondCard { get; set; }
        public string SendSecondCardBy { get; set; }
        public DateTime? SendThirdCard { get; set; }
        public string SendThirdCardBy { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string HelperInd { get; set; }
        public string HelperEmail { get; set; }
        public string HelperFirstName { get; set; }
        public string HelperLastName { get; set; }
        public string EtiologyName { get; set; }
        public string PromptCategory { get; set; }
        public string PaymentType { get; set;}
    }
}
