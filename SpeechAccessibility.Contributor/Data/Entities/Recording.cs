using SpeechAccessibility.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechAccessibility.Models
{
    public class Recording
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public Prompt OriginalPrompt { get; set; }
        public string ModifiedTranscript { get; set; }
        public Guid ContributorId { get; set; }
        public Block Block { get; set; }
        public string RatingBy { get; set; }
        public string Comment { get; set; }
        public int RetryCount { get; set; }

        public RecordingStatus Status { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateTS { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateTS { get; set; }

        public string LastUpdateBy { get; set; }

        public string ClientStartTS { get; set; }

        public string ClientEndTS { get; set; }
    }
}
