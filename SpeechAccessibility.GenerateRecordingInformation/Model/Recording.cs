namespace SpeechAccessibility.GenerateRecordingInformation.Model
{
    public class Recording
    {

        public int Id { get; set; }
        public string FileName { get; set; }
        public int? BlockId { get; set; }
        public int OriginalPromptId { get; set; }
        public Guid ContributorId { get; set; }
        public DateTime CreateTS { get; set; }
        public DateTime UpdateTS { get; set; }
        public string LastUpdateBy { get; set; }

        public string StartTime { get; set; }
        public string EndTime { get; set; }

    }
}
