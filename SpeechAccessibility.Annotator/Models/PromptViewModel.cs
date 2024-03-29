﻿using Microsoft.AspNetCore.Mvc.Rendering;
using SpeechAccessibility.Core.Models;
using System.Collections.Generic;

namespace SpeechAccessibility.Annotator.Models
{
    public class PromptViewModel
    {
        public Prompt Prompt { get; set; }
      
        public int EtioglogyId { get; set; }
        public List<int> ExistingEtioglogyIds { get; set; }
        public string Action { get; set; }
        public List<SelectListItem> Categories { set; get; }
        public List<SelectListItem> SubCategories { set; get; }
        public List<SelectListItem> Etiologies { set; get; }

    }
}
