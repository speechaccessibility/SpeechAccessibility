using SpeechAccessibility.Core.Models;
using System.Collections.Generic;

namespace SpeechAccessibility.Annotator.Models
{
    public class AnnotatorAssignedContributorsViewModel
    {
        public int AnnotatorId { get; set; }
        public List<Contributor> AssignedContributors { get; set; }
        
    }
}
