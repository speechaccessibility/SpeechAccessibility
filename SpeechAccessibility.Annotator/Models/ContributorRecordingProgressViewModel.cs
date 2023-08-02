using System;

namespace SpeechAccessibility.Annotator.Models
{
    public class ContributorRecordingProgressViewModel
    {
        public Guid ContributorId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime ApproveTS { get; set; }
        public DateTime RecordCreateTS { get; set; }
        public string BlockDescription { get; set; }

    }
}
