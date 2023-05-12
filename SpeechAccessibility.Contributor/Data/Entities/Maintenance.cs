using System;

namespace SpeechAccessibility.Data.Entities
{
    public class Maintenance
    {

        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool ContributorInd { get; set; }
        public bool AnnotatorInd { get; set; }
    }
}
