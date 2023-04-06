using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SpeechAccessibility.Annotator.Extensions;
using SpeechAccessibility.Annotator.Models;
using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Controllers
{
    [Authorize(Policy = "AnnotatorAdmin")]
    public class PromptController : Controller
    {
        private readonly IPromptRepository _promptRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISubCategoryRepository _subCategoryRepository;
        private readonly IRecordingRepository _recordingRepository;


        public PromptController(IPromptRepository promptRepository, IUserRepository userRepository, ICategoryRepository categoryRepository, ISubCategoryRepository subCategoryRepository, IRecordingRepository recordingRepository)
        {
            _promptRepository = promptRepository;
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
            _subCategoryRepository = subCategoryRepository;
            _recordingRepository = recordingRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoadPrompts(string status)
        {
            var loggedInUser = _userRepository.Find(u => u.NetId == User.Identity.Name).FirstOrDefault();

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;


            var prompts = _promptRepository.Find(p => p.Active == status).Include(p=>p.Category)
                .Include(p=>p.Recording).Include(p=>p.BlockMasterOfPrompts).Include(p=>p.BlockOfDigitalCommandPrompts)
                .Where(p=>p.Transcript.Contains(searchValue) || p.Category.Description.Contains(searchValue) || p.Id.ToString().Contains(searchValue));
            var recordsTotal = prompts.Count();

            prompts = DynamicSortingExtensions<Prompt>.SetOrderByDynamic(prompts, Request.Form);
            var promptList = prompts.Skip(skip).Take(pageSize).Select(p => new Prompt()
            {
                Id = p.Id,
                Transcript=p.Transcript,
                CreateTS =p.CreateTS,
                UpdateTS=p.UpdateTS,
                Category =p.Category,
                Recording = p.Recording,
                Active = p.Active,
                InUsed = p.Recording.Count>0,
                CanNotDelete = p.BlockMasterOfPrompts.Count > 0 || p.BlockOfDigitalCommandPrompts.Count > 0

            });
           

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = promptList });

        }

        public ActionResult AddUpdatePrompt(int promptId)
        {

            var promptVM = new PromptViewModel();
            var categories = _categoryRepository.Find(c => c.Active == "Yes").OrderBy(c => c.Description).Select(c =>
                    new SelectListItem()
                    {
                        Value = c.Id.ToString(),
                        Text = c.Description
                    })
                .ToList();
            List<SelectListItem> subCategories;
            var prompt = _promptRepository.Find(p => p.Id == promptId).FirstOrDefault();
            if (prompt == null)
            {
                prompt = new Prompt
                {
                    Id = 0,
                    SubCategoryId = 0,
                    CategoryId = Convert.ToInt32(categories[0].Value),
                    QuestionType = "DropDown"
                };
                subCategories = _subCategoryRepository.Find(c => c.CategoryId == Convert.ToInt64(categories[0].Value) && c.Active == "Yes").OrderBy(c => c.Description).Select(c =>
                        new SelectListItem()
                        {
                            Value = c.Id.ToString(),
                            Text = c.Description
                        })
                    .ToList();
            }
            else
            {

                subCategories = _subCategoryRepository.Find(c => c.CategoryId == prompt.CategoryId && c.Active == "Yes").OrderBy(c => c.Description).Select(c =>
                        new SelectListItem()
                        {
                            Value = c.Id.ToString(),
                            Text = c.Description
                        })
                    .ToList();

            }
            
            promptVM.Categories = categories;
            promptVM.SubCategories = subCategories;

            promptVM.Prompt = prompt;
            return PartialView("_AddUpdatePrompt", promptVM);


        }

     
        public JsonResult GetSubCategories(int categoryId)
        {
            var subCategories = _subCategoryRepository.Find(c => c.CategoryId == categoryId && c.Active == "Yes").OrderBy(c => c.Description).Select(c =>
                    new SelectListItem()
                    {
                        Value = c.Id.ToString(),
                        Text = c.Description
                    })
                .ToList();
            return Json(subCategories);

        }

        [HttpPost]
        public ActionResult AddUpdatePrompt(Prompt promptIn, string action)
        {
            //check to make sure the transcript is not in the database to avoid duplicates
            var existingPrompt = _promptRepository.Find(p => p.Transcript.Equals(promptIn.Transcript))
                .FirstOrDefault();
            if (existingPrompt != null)
            {
                return Json(new { Success = false, Message = "The transcript already exists in the database." });
            }


            if (action == "addnew")
            {
                if (promptIn.SubCategoryId == 0)
                {
                    promptIn.SubCategoryId = null;

                }

                if (promptIn.CategoryId > 1) //not the Dysarthria Assessment Sentences
                {
                    promptIn.QuestionType = "DropDown";
                }
                else
                {
                    if (string.IsNullOrEmpty(promptIn.QuestionType))
                    {
                        promptIn.QuestionType = "DropDown";
                    }
                }

                promptIn.CreateTS=DateTime.Now;
                promptIn.UpdateBy = User.Identity.Name;
                _promptRepository.Insert(promptIn);
                return Json(new { Success = true, Message = "Prompt is added." });
            }

            var prompt = _promptRepository.Find(p => p.Id == promptIn.Id).Include(p=>p.BlockMasterOfPrompts).Include(p=>p.BlockOfDigitalCommandPrompts).FirstOrDefault();
            if (prompt == null)
            {
                return Json(new { Success = false, Message = "Could not find the Prompt" });
            }


            if (action == "update")
            {
                prompt.Transcript = promptIn.Transcript;
                prompt.CategoryId = promptIn.CategoryId;
                if (promptIn.SubCategoryId > 0)
                {
                    prompt.SubCategoryId= promptIn.SubCategoryId;
                }
                else
                {
                    prompt.SubCategoryId = null;
                }

                prompt.QuestionType = promptIn.QuestionType;
                
                prompt.UpdateTS = DateTime.Now;
                prompt.UpdateBy = User.Identity.Name;
                _promptRepository.Update(prompt);
                return Json(new { Success = true, Message = "Prompt is updated." });
            }

            if (action == "archive")
            {
                if (prompt.BlockMasterOfPrompts.Count > 0 || prompt.BlockOfDigitalCommandPrompts.Count > 0)
                {
                    return Json(new { Success = false, Message = "This prompt cannot be archived because it is in the predefined list." });
                }

                prompt.Active = "No";
                prompt.UpdateTS = DateTime.Now;
                prompt.UpdateBy = User.Identity.Name;
                _promptRepository.Update(prompt);
                return Json(new { Success = true, Message = "Prompt is archived." });

            }

            if (action == "delete")
            {
                if (prompt.BlockMasterOfPrompts.Count > 0 || prompt.BlockOfDigitalCommandPrompts.Count > 0)
                {
                    return Json(new { Success = false, Message = "This prompt cannot be deleted because it is in the predefined list." });
                }
                //check to make sure if there are no recording related to this prompt.
                // if there are, only archive
                var existingRecordings = _recordingRepository.Find(r => r.OriginalPromptId == prompt.Id);
               if(existingRecordings.Any())
               {
                   prompt.Active = "No";
                   prompt.UpdateTS = DateTime.Now;
                   _promptRepository.Update(prompt);
                   return Json(new { Success = true, Message = "There are existing recordings using the prompt. The prompt is archived." });
                } 
               _promptRepository.Delete(prompt);
               return Json(new { Success = true, Message = "Prompt is deleted." });
            }

            if (action == "setActive")
            {
                prompt.Active = "Yes";
                prompt.UpdateTS = DateTime.Now;
                prompt.UpdateBy = User.Identity.Name;
                _promptRepository.Update(prompt);
                return Json(new { Success = true, Message = "Prompt is now active." });
            }



            return Json(new { Success = false, Message = "" });

        }




    }
}
