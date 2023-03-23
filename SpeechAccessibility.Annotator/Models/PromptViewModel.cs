using Microsoft.AspNetCore.Mvc.Rendering;
using SpeechAccessibility.Core.Models;
using System.Collections.Generic;

namespace SpeechAccessibility.Annotator.Models
{
    public class PromptViewModel
    {
        public Prompt Prompt { get; set; }
      
        public List<SelectListItem> Categories { set; get; }
        public List<SelectListItem> SubCategories { set; get; }

    }
}
