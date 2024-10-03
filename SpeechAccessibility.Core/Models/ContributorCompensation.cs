using System;

namespace SpeechAccessibility.Core.Models
{
    public class ContributorCompensation
    {
        public int Id { get; set; }
        public Guid ContributorId { get; set; }
        public DateTime? SendFirstCard { get; set; }
        public string SendFirstCardBy { get; set; }
        public DateTime? SendSecondCard { get; set; }
        public string SendSecondCardBy { get; set; }
        public DateTime? SendThirdCard { get; set; }
        public string SendThirdCardBy { get; set; }
        public string PaymentType { get; set; }
        public string PaidHelper { get; set; }
    }
}
