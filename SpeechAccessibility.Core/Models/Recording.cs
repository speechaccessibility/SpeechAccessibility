using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace SpeechAccessibility.Core.Models
{
  public  class Recording
    {
        public Recording()
        {
            RecordingRating = new HashSet<RecordingRating>();
        }

        public int Id { get; set; }
        public string FileName { get; set; }
        public int OriginalPromptId { get; set; }
        public string ModifiedTranscript { get; set; }
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

        public  Block Block { get; set; }
        public  Prompt OriginalPrompt { get; set; }
        public  RecordingStatus Status { get; set; }
        public  ICollection<RecordingRating> RecordingRating { get; set; }


        //speech file path
        [NotMapped]
        public string SpeechFilePath { get; set; }
        [NotMapped]
        public bool IsContributorApproved { get; set; }

        [NotMapped]
        public string EtiologyName { get; set; }
        [NotMapped]
        public int EtiologyId { get; set; }


    }
}
