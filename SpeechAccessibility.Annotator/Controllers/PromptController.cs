using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SpeechAccessibility.Annotator.Extensions;
using SpeechAccessibility.Annotator.Models;
using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;
using SpeechAccessibility.Infrastructure.Data;

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
        private readonly IPromptEtiologyRepository _promptEtiologyRepository;
        private readonly IEtiologyViewRepository _etiologyRepository;


        public PromptController(IPromptRepository promptRepository, IUserRepository userRepository, ICategoryRepository categoryRepository
            , ISubCategoryRepository subCategoryRepository, IRecordingRepository recordingRepository, IPromptEtiologyRepository promptEtiologyRepository
            , IEtiologyViewRepository etiologyRepository)
        {
            _promptRepository = promptRepository;
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
            _subCategoryRepository = subCategoryRepository;
            _recordingRepository = recordingRepository;
            _promptEtiologyRepository = promptEtiologyRepository;
            _etiologyRepository = etiologyRepository;
        }

        public IActionResult Index()
        {
            //ViewBag.EtiologyList = _epiologyRepository.Find(r => r.Active == "Yes").ToList();
            ViewBag.EtiologyList = new SelectList(_etiologyRepository.Find(r => r.Active == "Yes").ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult LoadPrompts(string status, int etiologyId)
        //public ActionResult LoadPrompts(string status)
        {
            var loggedInUser = _userRepository.Find(u => u.NetId == User.Identity.Name).FirstOrDefault();

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            //var prompts = _promptRepository.Find(p => p.Active == status).Include(p => p.Category)
            //    .Include(p => p.Recording).Include(p => p.BlockMasterOfPrompts).Include(p => p.BlockOfDigitalCommandPrompts)
            //    .Where(p => p.Transcript.Contains(searchValue) || p.Category.Description.Contains(searchValue) || p.Id.ToString().Contains(searchValue));

            //var prompts = _promptRepository.Find(p => p.Active == status)

            //    .Include(p => p.Category)
            //    .Include(p => p.Recording).Include(p => p.BlockMasterOfPrompts)
            //    .Include(p => p.BlockOfDigitalCommandPrompts)
            //    .Where(p => p.Transcript.Contains(searchValue) || p.Category.Description.Contains(searchValue) || p.Id.ToString().Contains(searchValue));

            var prompts = _promptEtiologyRepository.Find(p => p.EtiologyId == etiologyId && p.Prompt.Active==status)
                .Include(p => p.Prompt)
                .Include(p => p.Prompt.Category)
                .Include(p => p.Prompt.Recording)
                .Include(p => p.Prompt.BlockMasterOfPrompts)
                .Include(p => p.Prompt.BlockOfDigitalCommandPrompts)
                .Where(p => p.Prompt.Transcript.Contains(searchValue) || p.Prompt.Category.Description.Contains(searchValue) || p.Prompt.Id.ToString().Contains(searchValue))
            ;
            var recordsTotal = prompts.Count();

            prompts = DynamicSortingExtensions<PromptEtiology>.SetOrderByDynamic(prompts, Request.Form);
            var promptList = prompts.Skip(skip).Take(pageSize).Select(p => new Prompt()
            {
                Id = p.Prompt.Id,
                Transcript = p.Prompt.Transcript,
                CreateTS = p.Prompt.CreateTS,
                UpdateTS = p.Prompt.UpdateTS,
                Category = p.Prompt.Category,
                Recording = p.Prompt.Recording,
                Active = p.Prompt.Active,
                InUsed = p.Prompt.Recording.Count > 0,
                CanNotDelete = p.Prompt.BlockMasterOfPrompts.Count > 0 || p.Prompt.BlockOfDigitalCommandPrompts.Count > 0

            });

            //prompts = DynamicSortingExtensions<Prompt>.SetOrderByDynamic(prompts, Request.Form);
            //var promptList = prompts.Skip(skip).Take(pageSize).Select(p => new Prompt()
            //{
            //    Id = p.Id,
            //    Transcript = p.Transcript,
            //    CreateTS = p.CreateTS,
            //    UpdateTS = p.UpdateTS,
            //    Category = p.Category,
            //    Recording = p.Recording,
            //    Active = p.Active,
            //    InUsed = p.Recording.Count > 0,
            //    CanNotDelete = p.BlockMasterOfPrompts.Count > 0 || p.BlockOfDigitalCommandPrompts.Count > 0

            //});


            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = promptList });

        }

        public ActionResult AddUpdatePrompt(int promptId, int etiologyId, string method)
        {

            var promptVM = new PromptViewModel();
            promptVM.Action = method;
            var categories = _categoryRepository.Find(c => c.Active == "Yes").OrderBy(c => c.Description).Select(c =>
                    new SelectListItem()
                    {
                        Value = c.Id.ToString(),
                        Text = c.Description
                    })
                .ToList();
            var etiologies = _etiologyRepository.Find(r => r.Active == "Yes").OrderBy(c => c.Name).Select(c =>
                    new SelectListItem()
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
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
            promptVM.Etiologies =etiologies;

            promptVM.Prompt = prompt;
            promptVM.EtioglogyId = method == "copy" ? 0 : etiologyId;
            promptVM.ExistingEtioglogyIds =
                _promptEtiologyRepository.Find(e => e.PromptId == promptId).Select(e => e.EtiologyId).ToList();
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
            if (action == "addnew" || action== "update")
            {
                //check to make sure the transcript is not in the database to avoid duplicates
                var existingPrompt = _promptRepository.Find(p => p.Transcript.Equals(promptIn.Transcript) && p.CategoryId == promptIn.CategoryId && p.SubCategoryId == promptIn.SubCategoryId)
                    .FirstOrDefault();
                if (existingPrompt != null)
                {
                    return Json(new { Success = false, Message = "The transcript already exists in the database." });
                }
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

                //new prompt, need to be added to the promptEtiology table
                var promptEtiology = new PromptEtiology
                {
                    PromptId = promptIn.Id,
                    EtiologyId = promptIn.EtioglogyId,
                    CreateTS = DateTime.Now
                };
                _promptEtiologyRepository.Insert(promptEtiology);

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


            if (action == "copy")
            {
                var promptEtiology = new PromptEtiology
                {
                    PromptId = promptIn.Id,
                    EtiologyId = promptIn.EtioglogyId,
                    CreateTS = DateTime.Now
                };
                _promptEtiologyRepository.Insert(promptEtiology);
                return Json(new { Success = true, Message = "Prompt is added." });
            }

            if (action == "delete")
            {
                //delete from PromptEtiology first
                var promptEtiology = _promptEtiologyRepository.Find(e => e.PromptId == promptIn.Id);
                _promptEtiologyRepository.RemoveRange(promptEtiology);
                _promptRepository.Delete(prompt);
                return Json(new { Success = true, Message = "Prompt is deleted." });
            }

            return Json(new { Success = false, Message = "" });

        }




    }
}
