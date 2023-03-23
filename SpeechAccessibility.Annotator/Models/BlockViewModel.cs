using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Models
{
    public class BlockViewModel
    {
        public  int Id { get; set; }
        public string Description { get; set; }
        public string Active { get; set; }

        public int CategoryId { get; set; }
        public List<SelectListItem> Blocks { set; get; }
        public List<SelectListItem> Categories { set; get; }
        public List<SelectListItem> SubCategories { set; get; }

        
    }
}
