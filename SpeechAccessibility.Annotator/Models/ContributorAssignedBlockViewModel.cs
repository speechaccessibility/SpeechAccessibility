using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Models
{
    public class ContributorAssignedBlockViewModel
    {
        public  Guid ContributorId { get; set; }
        public  Contributor Contributor { get; set; }
        public  int BlockId { get; set; }
        public string InUsed { get; set; }
        public  List<Block> AssignedBlocks { get; set; }
        //public List<SelectListItem> AllBlocks { get; set; }
    }
}
