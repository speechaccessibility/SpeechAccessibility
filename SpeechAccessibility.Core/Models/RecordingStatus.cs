using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
    public class RecordingStatus 
    {
        public RecordingStatus()
        {
            Recording = new HashSet<Recording>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public  ICollection<Recording> Recording { get; set; }
    }
}
