using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Models
{
    public class ContributorAssignedAnnotatorViewModel
    {
        public Contributor Contributor { get; set; }
        public int AnnotatorId { get; set; }
        public List<ContributorAssignedAnnotator> ContributorAssignedAnnotators { get; set; }
        public List<SelectListItem> ExistingAnnotators { set; get; }
    }

}
