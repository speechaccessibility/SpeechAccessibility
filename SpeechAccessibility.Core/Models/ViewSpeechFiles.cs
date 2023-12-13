using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpeechAccessibility.Core.Models
{
    public class ViewSpeechFiles
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public int OriginalPromptId { get; set; }
        public string ModifiedTranscript { get; set; }
        public string OriginalTranscript { get; set; }
        public Guid ContributorId { get; set; }
        public int StatusId { get; set; }
        public int? BlockId { get; set; }
        public string RatingBy { get; set; }
        public string Comment { get; set; }
        public int RetryCount { get; set; }
        public DateTime CreateTS { get; set; }
        public DateTime UpdateTS { get; set; }
        public string LastUpdateBy { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string ClientStartTS { get; set; }
        public string ClientEndTS { get; set; }
        public int ContributorStatusId { get; set; }
        public string EtiologyName { get; set; }
        public string CategoryName { get; set; }

        //speech file path
        [NotMapped]
        public string SpeechFilePath { get; set; }
        [NotMapped]
        public bool IsContributorApproved { get; set; }

    }
}
