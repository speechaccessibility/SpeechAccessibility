namespace SpeechAccessibility.Core.Models
{
    public class NumberOfRecordingByEtiology
    {
        //public int? NumberOfRecordings { get; set; }
        public int Id { get; set; }
        public int EtiologyId { get; set; }
        public int? CategoryId { get; set; }
    }
}
