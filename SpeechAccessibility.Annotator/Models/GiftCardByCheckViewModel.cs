using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpeechAccessibility.Annotator.Models
{
    public class GiftCardByCheckViewModel
    {
        public double Amount { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime FirstRecordingDate { get; set; }
        public DateTime LastRecordingDate { get; set; }
    }
}
