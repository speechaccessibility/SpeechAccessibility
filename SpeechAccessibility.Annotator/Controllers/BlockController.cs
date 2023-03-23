using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SpeechAccessibility.Annotator.Extensions;
using SpeechAccessibility.Annotator.Models;
using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Controllers
{
    [Authorize(Policy = "AllAnnotator")]
    public class BlockController : Controller
    {
        
      
       private readonly IBlockMasterRepository _blockMasterRepository;
       private readonly IBlockMasterOfPromptsRepository _blockMasterOfPromptsRepository;
       private readonly ICategoryRepository _categoryRepository;
       private readonly IPromptRepository _promptRepository;
        public BlockController(IBlockMasterRepository blockMasterRepository, IBlockMasterOfPromptsRepository blockMasterOfPromptsRepository, ICategoryRepository categoryRepository, IPromptRepository promptRepository)
        {
            _blockMasterRepository = blockMasterRepository;
            _blockMasterOfPromptsRepository = blockMasterOfPromptsRepository;
            _categoryRepository = categoryRepository;
            _promptRepository = promptRepository;
        }

        public IActionResult Index()
        {
            var blockVM = new BlockViewModel();
            var blocks = _blockMasterRepository.Find(b=>b.Active=="Yes").OrderBy(b=>b.Description).Select(c =>
                    new SelectListItem()
                    {
                        Value = c.Id.ToString(),
                        Text = c.Description
                    })
                .ToList();
            //should not include the "Dysarthria Assessment Sentences"
            var categories = _categoryRepository.Find(c => c.Active == "Yes" && c.Id!=1).OrderBy(c => c.Id).Select(c =>
                    new SelectListItem()
                    {
                        Value = c.Id.ToString(),
                        Text = c.Description
                    })
                .ToList();
            blockVM.Blocks=blocks;
            blockVM.Categories = categories;
            return View(blockVM);
        }
        [HttpPost]

        [HttpPost]
        public JsonResult GetPromptList(string search, int categoryId)
        {
            var searchResults = _promptRepository.Find(p => p.Active == "Yes" && p.CategoryId==categoryId && p.Transcript.Contains(search)).OrderBy(p=>p.Transcript).Take(15);
            return Json(searchResults);
        }

        public ActionResult LoadMasterOfPrompts()
        {
          
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            //currently only Block # 1 (id=1) is static and can be modified. 
           
            var blockOnePrompts = _blockMasterOfPromptsRepository.Find(p => p.BlockMaster.Active == "Yes")
                .Include(p => p.BlockMaster)
                .Include(p => p.Prompt).ThenInclude(p=>p.Category)
                .Where(p => p.Prompt.Transcript.Contains(searchValue) || p.Category.Description.Contains(searchValue) ||p.BlockMaster.Description.Contains(searchValue));

            var recordsTotal = blockOnePrompts.Count();

            var promptList = blockOnePrompts.Skip(skip).Take(pageSize).Select(p => new BlockMasterOfPrompts()
            {
                Id = p.Id,
               Prompt = p.Prompt,
               BlockMaster = p.BlockMaster


            });
            promptList = DynamicSortingExtensions<BlockMasterOfPrompts>.SetOrderByDynamic(promptList, Request.Form);


            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = promptList });

        }

        [Authorize(Policy = "TextAnnotatorAdmin")]
        [HttpPost]
        public ActionResult AddPromptToBlock(int blockMasterId, int categoryId, int promptId)
        {
            if(blockMasterId<1 || categoryId<1 || promptId < 1)
                return Json(new { Success = false, Message = "Missing required fields." });

            var newMasterPrompt = new BlockMasterOfPrompts
            {
                BlockMasterId = blockMasterId,
                CategoryId = categoryId,
                PromptId = promptId
            };
            _blockMasterOfPromptsRepository.Insert(newMasterPrompt);
            return Json(new { Success = true, Message = "" });
        }

        [Authorize(Policy = "TextAnnotatorAdmin")]
        [HttpPost]
        public ActionResult RemovePromptToBlock(int blockMasterPromptId)
        {
            var existingPrompt =
                _blockMasterOfPromptsRepository.Find(p => p.Id == blockMasterPromptId).FirstOrDefault();
            if (existingPrompt == null)
                return Json(new { Success = false, Message = "Record not found." });

           
            _blockMasterOfPromptsRepository.Delete(existingPrompt);
            return Json(new { Success = true, Message = "" });
        }

    }
}
