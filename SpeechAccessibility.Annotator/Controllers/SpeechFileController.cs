using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SpeechAccessibility.Annotator.Extensions;
using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;
using SpeechAccessibility.Annotator.Models;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

//using System.Configuration;

namespace SpeechAccessibility.Annotator.Controllers
{
    [Authorize(Policy = "AllAnnotatorAndExternalSLPAnnotator")]
    public class SpeechFileController : Controller
    {

        
        private readonly IRecordingRepository _recordingRepository;
        private readonly IRecordingRatingRepository _recordingRatingRepository;
        private readonly IDimensionCategoryRepository _dimensionCategoryRepository;
        private readonly IContributorViewRepository _contributorViewRepository;
        private readonly IContributorAssignedBlockRepository _contributorAssignedBlockRepository;
        private readonly IBlockOfPromptsRepository _blockOfPromptsRepository;
        private readonly IContributorAssignedAnnotatorRepository _contributorAssignedAnnotatorRepository;
        private readonly IBlockRepository _blockRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IUserSubRoleRepository _userSubRoleRepository;
        private readonly IViewSpeechFilesRepository _viewSpeechFilesRepository;


        public SpeechFileController(IRecordingRepository recordingRepository, IRecordingRatingRepository recordingRatingRepository,IDimensionCategoryRepository dimensionCategoryRepository
            , IContributorViewRepository contributorViewRepository, IContributorAssignedBlockRepository contributorAssignedBlockRepository, IBlockOfPromptsRepository blockOfPromptsRepository
            , IContributorAssignedAnnotatorRepository contributorAssignedAnnotatorRepository, IUserRepository userRepository, IBlockRepository blockRepository, IConfiguration configuration
            , IUserSubRoleRepository userSubRoleRepository, IViewSpeechFilesRepository viewSpeechFilesRepository)
        {
            _recordingRepository = recordingRepository;
            _recordingRatingRepository = recordingRatingRepository;
            _dimensionCategoryRepository = dimensionCategoryRepository;
            _contributorViewRepository = contributorViewRepository;
            _contributorAssignedBlockRepository = contributorAssignedBlockRepository;
            _blockOfPromptsRepository = blockOfPromptsRepository;
            _contributorAssignedAnnotatorRepository = contributorAssignedAnnotatorRepository;
            _userRepository = userRepository;
            _blockRepository = blockRepository;
            _configuration = configuration;
            _userSubRoleRepository = userSubRoleRepository;
            _viewSpeechFilesRepository = viewSpeechFilesRepository;
        }

        public IActionResult Index()
        {
            return View();
        }
        [Authorize(Policy = "SLPAnnotatorAndTextAnnotatorAdmin")]
        public IActionResult AllSITSpeechFiles()
        {
            return View();
        }


        [HttpPost]
        public PartialViewResult LoadContributorOriginalSpeechFiles(Guid contributorId)
        {
            var recordingList = new List<Recording>();
            var recordings = _recordingRepository
                .Find(r => r.ContributorId == contributorId && r.OriginalPrompt.CategoryId != 1).Include(r => r.OriginalPrompt)
                .ThenInclude(c=>c.Category).ThenInclude(c=>c.SubCategory).OrderByDescending(c=>c.CreateTS).ToList();

            var appName = _configuration["AppSettings:Environment"] switch
            {
                "Development" => _configuration["AppSettings:AnnotatorWebLink"] + "/UploadFiles/Dev/",
                "Test" => _configuration["AppSettings:AnnotatorWebLink"] + "/UploadFiles/Test/",
                _ => _configuration["AppSettings:AnnotatorWebLink"] + "/UploadFiles/Prod/"
            };

            foreach (var recording in recordings)
            {
                var r = new Recording
                {
                    Id = recording.Id,
                    OriginalPrompt = recording.OriginalPrompt,
                    
                    ModifiedTranscript = recording.ModifiedTranscript,
                    CreateTS = recording.CreateTS,
                    RetryCount = recording.RetryCount,
                    Comment = recording.Comment,
                    ContributorId = recording.ContributorId,
                    SpeechFilePath = appName + recording.ContributorId + "/" + recording.BlockId + "/raw/" + recording.FileName
                 };
                recordingList.Add(r);
            }
            return PartialView("_ContributorOriginalSpeechFiles", recordingList);
           
        }
        [HttpPost]
        public PartialViewResult LoadContributorAssignedBlocks(Guid contributorId,int subRole)
        {
            //if external SLP annotator, check to make sure the role is matched
            var hasSubRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.OtherPhone)?.Value;
            if (hasSubRole == "Yes")
            {
                if (UtilsExtension.IsMatchedRole(_userSubRoleRepository, subRole, User.Identity.Name) == false)
                    return PartialView("_ContributorAssignedBlocks", null);

            }

            //if this contributor is not assigned to current Annotator, return null
            var currentUser = _userRepository.Find(u => u.NetId == User.Identity.Name).FirstOrDefault();
            var userRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            
            var assignedBlocks = new List<ContributorAssignedBlock>();
            if (userRole == "TextAnnotator") 
            {
                var isAssigned = _contributorAssignedAnnotatorRepository
                    .Find(c => c.ContributorId == contributorId && c.UserId == currentUser.Id).FirstOrDefault();
                if(isAssigned is null)
                    return PartialView("_ContributorAssignedBlocks", assignedBlocks);

            }

            var blocks = _contributorAssignedBlockRepository.Find(b => b.ContributorId == contributorId)
                .Include(b => b.Block).ThenInclude(b => b.BlockOfPrompts);

            foreach (var block in blocks)
            {
                var assignedBlock = new ContributorAssignedBlock
                {
                    Id = block.Id,
                    ContributorId = block.ContributorId,
                    BlockId = block.BlockId,
                    Block = block.Block
                };
                assignedBlocks.Add(assignedBlock);
            }
            return PartialView("_ContributorAssignedBlocks", assignedBlocks);

        }

        [HttpPost]
        public ActionResult LoadSpeechFiles(int recordingStatus)
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var appName = _configuration["AppSettings:Environment"] switch
            {
                "Development" => _configuration["AppSettings:AnnotatorWebLink"] + "/UploadFiles/Dev/",
                "Test" => _configuration["AppSettings:AnnotatorWebLink"] + "/UploadFiles/Test/",
                _ => _configuration["AppSettings:AnnotatorWebLink"] + "/UploadFiles/Prod/"
            };

            //IQueryable<Recording> recordings;
            IQueryable<ViewSpeechFiles> recordings;
            var userRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            List<int> includeStatus = new List<int>();
            //if recordingStatus=2, include the new recording
            if (recordingStatus == 2)
            {
                includeStatus.Add(1);
                includeStatus.Add(2);
            }
            else
            {
                includeStatus.Add(recordingStatus);
            }

            //for TextAnnotator, only return assigned recordings
            if (userRole == "TextAnnotator")
            {
                var currentUser = _userRepository.Find(u => u.NetId == User.Identity.Name).FirstOrDefault();
                Guid[] annotatorAssignedContributorIdList = _contributorAssignedAnnotatorRepository.Find(c => c.UserId == currentUser.Id)
                    .Select(u => u.ContributorId).ToArray();

                //get recordings that are not Dysarthria Assessment Sentences (categoryId=1)
                recordings = _viewSpeechFilesRepository
                    .Find(r => r.ContributorStatusId == 2 && includeStatus.Contains(r.StatusId)
                                                          && annotatorAssignedContributorIdList.Contains(r.ContributorId))
                    .Where(r => r.ContributorId.ToString().Contains(searchValue) || r.ModifiedTranscript.Contains(searchValue)
                        || r.CategoryName.Contains(searchValue) || r.Id.ToString().Contains(searchValue) || r.Comment.Contains(searchValue));

            }
            else
            {
                //get recordings that are not Dysarthria Assessment Sentences (categoryId=1)
                recordings = _viewSpeechFilesRepository
                    .Find(r => r.ContributorStatusId == 2 && includeStatus.Contains(r.StatusId))
                    .Where(r => r.ContributorId.ToString().Contains(searchValue) || r.ModifiedTranscript.Contains(searchValue)
                        || r.CategoryName.Contains(searchValue) || r.Id.ToString().Contains(searchValue) || r.Comment.Contains(searchValue));

            }

          

            var recordsTotal = recordings.Count();
            recordings = DynamicSortingExtensions<ViewSpeechFiles>.SetOrderByDynamic(recordings, Request.Form);
            //recordings = recordings.Skip(skip).Take(pageSize);
            recordings = recordings.Skip(skip).Take(pageSize).Select(r => new ViewSpeechFiles()
            {
                Id=r.Id,
                SpeechFilePath = appName + r.ContributorId + "/" + r.BlockId + "/modified/" + r.FileName,
                OriginalPromptId = r.OriginalPromptId,
                ModifiedTranscript = r.ModifiedTranscript,
                OriginalTranscript = r.OriginalTranscript,
                ContributorId = r.ContributorId,
                StatusId = r.StatusId,
                BlockId = r.BlockId,
                RatingBy = r.RatingBy,
                Comment = r.Comment,
                CreateTS = r.CreateTS,
                UpdateTS = r.UpdateTS,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                FileName = r.FileName,
                EtiologyName = r.EtiologyName,
                CategoryName = r.CategoryName,
                IsContributorApproved =r.ContributorStatusId != 1

            });

          
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = recordings });
        }
        

        [HttpPost]
        public ActionResult AddCommentsForRecording(int recordingId, string comments)
        {
            var recording =_recordingRepository.Find(r=>r.Id==recordingId).FirstOrDefault();
            if (recording != null)
            {
              
                recording.Comment = comments;
                recording.UpdateTS = DateTime.Now;
                recording.LastUpdateBy = User.Identity.Name;
               
                _recordingRepository.Update(recording);
                return Json(new { Success = true, Message = "" });
            }
            return Json(new { Success = false, Message = "Error adding comments." });
        }

        [Authorize(Policy = "SLPAnnotatorAndTextAnnotatorAdminAndExternalSLPAnnotator")]
        [HttpPost]
        public PartialViewResult LoadContributorSITRecordings(Guid contributorId, int subRole)
        {
            //if external SLP annotator, check to make sure the role is matched
            var hasSubRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.OtherPhone)?.Value;
            if (hasSubRole == "Yes")
            {
                if (UtilsExtension.IsMatchedRole(_userSubRoleRepository, subRole, User.Identity.Name) == false)
                    return PartialView("_ContributorSITSpeechFiles", null);

            }
            var recordingList = new List<Recording>();
            var recordings = _recordingRepository
                .Find(r => r.ContributorId == contributorId && r.OriginalPrompt.CategoryId == 1).Include(r => r.OriginalPrompt)
                .ToList();
            var contributor = _contributorViewRepository.Find(r => r.Id == contributorId).FirstOrDefault();
            bool isApproved = contributor != null && contributor.StatusId != 1;

            var appName = _configuration["AppSettings:Environment"] switch
            {
                "Development" => _configuration["AppSettings:AnnotatorWebLink"] + "/UploadFiles/Dev/",
                "Test" => _configuration["AppSettings:AnnotatorWebLink"] + "/UploadFiles/Test/",
                _ => _configuration["AppSettings:AnnotatorWebLink"] + "/UploadFiles/Prod/"
            };
            foreach (var recording in recordings)
            {
                var r = new Recording
                {
                    Id = recording.Id,
                    OriginalPrompt = recording.OriginalPrompt,

                    ModifiedTranscript = recording.ModifiedTranscript,
                    CreateTS = recording.CreateTS,
                    RetryCount = recording.RetryCount,
                    Comment = recording.Comment,
                    ContributorId = recording.ContributorId,
                    SpeechFilePath = appName + recording.ContributorId + "/0/modified/" + recording.FileName,
                    IsContributorApproved = isApproved
                };
                recordingList.Add(r);
            }
            return PartialView("_ContributorSITSpeechFiles", recordingList);

         
        }

        [Authorize(Policy = "TextAnnotator")]
        [HttpPost]
        public ActionResult UpdateRecordingTranscript(int recordingId, string transcript, string startTime, string endTime)
        {
            var recording = _recordingRepository.Find(p => p.Id == recordingId).FirstOrDefault();
            if (recording == null)
            {
                return Json(new { Success = false, Message = "Could not find the Recording" });
            }

            if (recording.StatusId == 1)
                recording.StatusId = 2;
            recording.ModifiedTranscript = transcript;
            recording.StartTime = startTime;
            recording.EndTime = endTime;

            recording.UpdateTS = DateTime.Now;
            recording.LastUpdateBy = User.Identity.Name;
            _recordingRepository.Update(recording);
            return Json(new { Success = true, Message = "Transcript is updated." });

        }

        [Authorize(Policy = "TextAnnotator")]
        [HttpPost]
        public ActionResult UpdateStatusRecording(int recordingId,string comment,  string action)
        {
            
            var recording = _recordingRepository.Find(p => p.Id == recordingId).FirstOrDefault();
            if (recording == null)
            {
                return Json(new { Success = false, Message = "Could not find the Speech File" });
            }

            //make sure this recording is assigned to the logged in user
            var userRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (userRole == "TextAnnotator")
            {
                var currentUser = _userRepository.Find(u => u.NetId == User.Identity.Name).FirstOrDefault();
                var assignedRecording = _contributorAssignedAnnotatorRepository.Find(c => c.UserId == currentUser.Id && c.ContributorId == recording.ContributorId).FirstOrDefault();
                if(assignedRecording is null)
                    return Json(new { Success = false, Message = "This recording is not assigned to you." });
                
            }


            if (action is "edit" or "unpublish")
            {
                recording.StatusId = 2;
                recording.Comment = comment;
                recording.UpdateTS = DateTime.Now;
                recording.LastUpdateBy = User.Identity.Name;
                _recordingRepository.Update(recording);
                var returnMessage = action == "unpublish" ? "Speech File is un-published" : "Speech File is un-excluded";

                return Json(new { Success = true, Message = returnMessage });
            }

            if (action == "exclude")
            {
                recording.StatusId = 4;
                recording.Comment = comment;
                
                recording.UpdateTS = DateTime.Now;
                recording.LastUpdateBy = User.Identity.Name;
                _recordingRepository.Update(recording);
                return Json(new { Success = true, Message = "Speech File is excluded." });

            }
            if (action == "publish")
            {
                recording.StatusId = 3;
                recording.Comment = comment;
                recording.UpdateTS = DateTime.Now;
                recording.LastUpdateBy = User.Identity.Name;
                _recordingRepository.Update(recording);
                return Json(new { Success = true, Message = "Speech File is published." });

            }
            if (action == "toDiscuss")
            {
                recording.StatusId = 5;
                recording.Comment = comment;
                recording.UpdateTS = DateTime.Now;
                recording.LastUpdateBy = User.Identity.Name;
                _recordingRepository.Update(recording);
                return Json(new { Success = true, Message = "Speech File is moved to To-Discuss list." });

            }
            if (action == "editComments")
            {
                if(recording.StatusId== 1)
                    recording.StatusId = 2;
                recording.Comment = comment;
                recording.UpdateTS = DateTime.Now;
                recording.LastUpdateBy = User.Identity.Name;
                _recordingRepository.Update(recording);
                return Json(new { Success = true, Message = "Comments are updated." });

            }
            return Json(new { Success = false, Message = "" });

        }

        public ActionResult ViewContributorRecordingsForBlock(Guid contributorId, int blockId)
        {
            //if this contributor is not assigned to current Annotator, return null
            var currentUser = _userRepository.Find(u => u.NetId == User.Identity.Name).FirstOrDefault();
            var userRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var recordingList = new List<Recording>();
            if (userRole == "TextAnnotator") //todo: may need to add SLP annotator here 
            {
                var isAssigned = _contributorAssignedAnnotatorRepository
                    .Find(c => c.ContributorId == contributorId && c.UserId == currentUser.Id).FirstOrDefault();
                if (isAssigned == null)
                    return PartialView("_ContributorRecordingsForBlock", recordingList);

            }

           
            //List<int> includeStatus = new List<int>() { 1, 2 ,3}; //new, edited, or published
            var recordings = _recordingRepository
                .Find(r => r.ContributorId == contributorId && r.BlockId==blockId).Include(r => r.OriginalPrompt)
                .ToList();

            var appName = _configuration["AppSettings:Environment"] switch
            {
                "Development" => _configuration["AppSettings:AnnotatorWebLink"] + "/UploadFiles/Dev/",
                "Test" => _configuration["AppSettings:AnnotatorWebLink"] + "/UploadFiles/Test/",
                _ => _configuration["AppSettings:AnnotatorWebLink"] + "/UploadFiles/Prod/"
            };
            foreach (var recording in recordings)
            {
                var r = new Recording
                {
                    Id = recording.Id,
                    OriginalPrompt = recording.OriginalPrompt,

                    ModifiedTranscript = recording.ModifiedTranscript,
                    CreateTS = recording.CreateTS,
                    RetryCount = recording.RetryCount,
                    Comment = recording.Comment,
                    ContributorId = recording.ContributorId,
                    SpeechFilePath = appName + recording.ContributorId + "/" + recording.BlockId + "/modified/" + recording.FileName
                };
                recordingList.Add(r);
            }

            return PartialView("_ContributorRecordingsForBlock", recordingList);
        }
        public ActionResult ViewAssignedBlockPrompts(int blockId)
        {
            var block = _blockRepository.Find(b => b.Id == blockId).FirstOrDefault();
            ViewBag.AssignedBlockDescription = block == null ? "" : block.Description;
            var assignedBlockPrompts =
                _blockOfPromptsRepository.Find(p=>p.BlockId==blockId)
                    .Include(p=>p.Prompt).ThenInclude(p=>p.Category)
                    .ThenInclude(p=>p.SubCategory).ToList();
            if (!assignedBlockPrompts.Any())
            {
                return PartialView("_ContributorAssignedPrompts", null);
            }

         
            return PartialView("_ContributorAssignedPrompts", assignedBlockPrompts);
        }
        public ActionResult ViewBlockRecordingsRating(Guid contributorId, int blockId)
        {
            var contributor = _contributorViewRepository.Find(c => c.Id == contributorId).FirstOrDefault();
            var block = _blockRepository.Find(b=>b.Id == blockId).FirstOrDefault();
            ViewBag.RatingContributorId = contributor.Id;
            ViewBag.RatingContributorLastName = contributor.LastName;
            ViewBag.RatingContributorFirstName = contributor.FirstName;
            ViewBag.BlockNumber = block.Description;

            var recordings =
                _recordingRepository.Find(r => r.ContributorId == contributorId && r.BlockId==blockId)
                    .Include(r => r.OriginalPrompt)
                    .Include(r => r.RecordingRating);
            if (!recordings.Any())
            {
                return PartialView("_ViewRatingSpeechFile", null);
            }

            List<RecordingRatingViewModel> listOfRecordingRating = new List<RecordingRatingViewModel>();
            foreach (var recording in recordings)
            {

                var recordingRatingList = _recordingRatingRepository.Find(r => r.RecordingId == recording.Id)
                    .Include(d => d.Dimension).ThenInclude(d => d.DimensionCategory).ToList();
                if (recordingRatingList.Count > 0)
                {
                    var recordingVM = new RecordingRatingViewModel();
                    recordingVM.Recording = recording;
                    recordingVM.RecordingRatingList = recordingRatingList;
                    listOfRecordingRating.Add(recordingVM);
                }


            }
            return PartialView("_ViewRatingSpeechFile", listOfRecordingRating);
        }
        public ActionResult ViewSITRecordingRating(Guid contributorId, int subRole)
        {
            //if external SLP annotator, check to make sure the role is matched
            var hasSubRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.OtherPhone)?.Value;
            if (hasSubRole == "Yes")
            {
                if (UtilsExtension.IsMatchedRole(_userSubRoleRepository, subRole, User.Identity.Name) == false)
                    return PartialView("_ViewRatingSpeechFile", null);

            }
            var contributor = _contributorViewRepository.Find(c => c.Id == contributorId).FirstOrDefault();
            ViewBag.RatingContributorId = contributor.Id;
            ViewBag.RatingContributorLastName = contributor.LastName;
            ViewBag.RatingContributorFirstName = contributor.FirstName;

            var recordings =
                _recordingRepository.Find(r => r.ContributorId == contributorId && r.OriginalPrompt.CategoryId == 1)
                    .Include(r => r.OriginalPrompt)
                    .Include(r => r.RecordingRating);
            if (!recordings.Any())
            {
                return PartialView("_ViewRatingSpeechFile", null);
            }

            List<RecordingRatingViewModel> listOfRecordingRating = new List<RecordingRatingViewModel>();
            foreach (var recording in recordings)
            {

                var recordingRatingList = _recordingRatingRepository.Find(r => r.RecordingId == recording.Id)
                    .Include(d => d.Dimension).ThenInclude(d => d.DimensionCategory).ToList();
                if (recordingRatingList.Count > 0)
                {
                    var recordingVM = new RecordingRatingViewModel();
                    recordingVM.Recording = recording;
                    recordingVM.RecordingRatingList = recordingRatingList;
                    listOfRecordingRating.Add(recordingVM);
                }


            }
            return PartialView("_ViewRatingSpeechFile", listOfRecordingRating);
        }

        [Authorize(Policy = "SLPAnnotator")] //clarion group
        [HttpPost]
        public ActionResult LoadSpeechFilesForRating()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;


            var recording = _recordingRepository.Find(r => r.StatusId == 4)
                .Where(r => r.ContributorId.ToString().Contains(searchValue) || r.ModifiedTranscript.Contains(searchValue) || r.OriginalPrompt.Category.Description.Contains(searchValue))
                .Include(r => r.OriginalPrompt).ThenInclude(p => p.Category);
            var recordsTotal = recording.Count();

            recording = (IIncludableQueryable<Recording, Category>)DynamicSortingExtensions<Recording>.SetOrderByDynamic(recording, Request.Form);

            var recordingList = recording.Skip(skip).Take(pageSize).Select(r => new Recording()
            {
                Id = r.Id,
                OriginalPromptId = r.OriginalPromptId,
                ContributorId = r.ContributorId,
                OriginalPrompt = r.OriginalPrompt,
                Status = r.Status,
                Comment = r.Comment,
                CreateTS = r.CreateTS,
                ModifiedTranscript = string.IsNullOrEmpty(r.ModifiedTranscript) ? r.OriginalPrompt.Transcript : r.ModifiedTranscript,
                FileName = r.FileName
            });
            

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = recordingList });

        }

        [Authorize(Policy = "SLPAnnotatorAndExternalSLPAnnotator")] //clarion group and ExternalSLPAnnotator (listen only)
        public ActionResult RateSpeechFiles(int recordingId)
        {
            //ViewBag.CameFrom = from;
            var recording = _recordingRepository.Find(r => r.Id == recordingId).Include(r => r.OriginalPrompt).FirstOrDefault();
            if (recording == null)
                return View("RateSpeechFiles", null);
            var dimensions = _dimensionCategoryRepository.Find(d => d.Active == "Yes")
                .Include(d => d.Dimension).OrderBy(d => d.DisplayOrder).ToList();
            var recordingRatingList = _recordingRatingRepository.Find(r => r.RecordingId == recordingId)
                .Include(d => d.Dimension).ThenInclude(d => d.DimensionCategory).ToList();

            var appName = _configuration["AppSettings:Environment"] switch
            {
                "Development" => _configuration["AppSettings:AnnotatorWebLink"] + "/UploadFiles/Dev/",
                "Test" => _configuration["AppSettings:AnnotatorWebLink"] + "/UploadFiles/Test/",
                _ => _configuration["AppSettings:AnnotatorWebLink"] + "/UploadFiles/Prod/"
            };

            recording.SpeechFilePath = recording.BlockId == null
                ? appName + recording.ContributorId + "/0/modified/" +
                  recording.FileName
                : appName + recording.ContributorId + "/" + recording.BlockId + "/modified/" +
                  recording.FileName;


            var recordingVM = new RecordingRatingViewModel();
            recordingVM.Recording = recording;
            recordingVM.RecordingRatingList = recordingRatingList;
            recordingVM.DimensionCategoryList = dimensions;

            return View("_RateSpeechFile", recordingVM);
            //return View();


        }
        [Authorize(Policy = "SLPAnnotator")]
        [HttpPost]
        public ActionResult RateSpeechFiles(Recording recordingForRating)
        {
           
            var recording = _recordingRepository.Find(p => p.Id == recordingForRating.Id).FirstOrDefault();
            if (recording == null)
            {
                return Json(new { Success = false, Message = "Could not find the Recording" });
            }
            //if there are rating for this recording, delete them and add new rating
            var existingRating = _recordingRatingRepository.Find(r => r.RecordingId == recordingForRating.Id);
            _recordingRatingRepository.RemoveRange(existingRating);

            foreach (var rating in recordingForRating.RecordingRating)
            {
                rating.RatingTS = DateTime.Now;
                rating.RatingBy = User.Identity.Name;
            }
            _recordingRatingRepository.AddRange(recordingForRating.RecordingRating);
          
            if ((!string.IsNullOrEmpty(recordingForRating.Comment) && !recordingForRating.Comment.Equals(recording.Comment)) || (string.IsNullOrEmpty(recordingForRating.Comment) && !string.IsNullOrEmpty(recording.Comment)))
            {
                recording.Comment = recordingForRating.Comment;
                recording.UpdateTS = DateTime.Now;
                recording.LastUpdateBy = User.Identity.Name;
                _recordingRepository.Update(recording);
            }

           
            return Json(new { Success = true, Message = "Recorded is rated." });

        }

        public IActionResult ModifiedSpeechFiles()
        {
            return View();
        }

        public IActionResult ExcludedSpeechFiles()
        {
            return View();
        }

        public IActionResult PublishedSpeechFiles()
        {
            return View();
        }

        public IActionResult ToDiscussSpeechFiles()
        {
            return View();
        }
        //[HttpPost]
        //public ActionResult LoadExcludedSpeechFiles()
        //{
        //    var draw = Request.Form["draw"].FirstOrDefault();
        //    var start = Request.Form["start"].FirstOrDefault();
        //    var length = Request.Form["length"].FirstOrDefault();
        //    var searchValue = Request.Form["search[value]"].FirstOrDefault();
        //    int pageSize = length != null ? Convert.ToInt32(length) : 0;
        //    int skip = start != null ? Convert.ToInt32(start) : 0;


        //    var approvedContributors = _contributorRepository.Find(c => c.StatusId == 2).ToList();
        //    Guid[] idList = approvedContributors.Select(c => c.Id).ToArray();

        //    var userRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        //    IQueryable<Recording> recordings;
        //    if (userRole == "TextAnnotator")
        //    {
        //        var currentUser = _userRepository.Find(u => u.NetId == User.Identity.Name).FirstOrDefault();
        //        Guid[] annotatorAssignedContributorIdList = _contributorAssignedAnnotatorRepository.Find(c => c.UserId == currentUser.Id)
        //            .Select(u => u.ContributorId).ToArray();

        //        recordings = _recordingRepository
        //            .Find(r => idList.Contains(r.ContributorId) && r.OriginalPrompt.CategoryId != 1 && r.StatusId==4
        //                       && annotatorAssignedContributorIdList.Contains(r.ContributorId))
        //            .Where(r => r.ContributorId.ToString().Contains(searchValue) || r.ModifiedTranscript.Contains(searchValue) || r.OriginalPrompt.Category.Description.Contains(searchValue))
        //            .Include(r => r.OriginalPrompt).ThenInclude(c => c.Category);
        //    }
        //    else
        //    {
        //        recordings = _recordingRepository
        //            .Find(r => idList.Contains(r.ContributorId) && r.OriginalPrompt.CategoryId != 1 && r.StatusId==4)
        //            .Where(r => r.ContributorId.ToString().Contains(searchValue) || r.ModifiedTranscript.Contains(searchValue) || r.OriginalPrompt.Category.Description.Contains(searchValue))
        //            .Include(r => r.OriginalPrompt).ThenInclude(c => c.Category);
        //    }

        //    //IQueryable<Recording> recording = _recordingRepository.Find(r => r.StatusId==4)
        //    //    .Where(r => r.ContributorId.ToString().Contains(searchValue) || r.ModifiedTranscript.Contains(searchValue) || r.OriginalPrompt.Category.Description.Contains(searchValue))
        //    //    .Include(r => r.OriginalPrompt).ThenInclude(p => p.Category);

        //    var recordsTotal = recordings.Count();

        //    var appName = _configuration["AppSettings:Environment"] switch
        //    {
        //        "Development" => _configuration["AppSettings:AnnotatorWebLink"] + "/UploadFiles/Dev/",
        //        "Test" => _configuration["AppSettings:AnnotatorWebLink"] + "/UploadFiles/Test/",
        //        _ => _configuration["AppSettings:AnnotatorWebLink"] + "/UploadFiles/Prod/"
        //    };

        //    recordings = DynamicSortingExtensions<Recording>.SetOrderByDynamic(recordings, Request.Form);

        //    var recordingList = recordings.Skip(skip).Take(pageSize).Select(r => new Recording()
        //    {
        //        Id = r.Id,
        //        OriginalPromptId = r.OriginalPromptId,
        //        ContributorId = r.ContributorId,
        //        OriginalPrompt = r.OriginalPrompt,
        //        Status = r.Status,
        //        Comment = r.Comment,
        //        CreateTS = r.CreateTS,
        //        ModifiedTranscript = string.IsNullOrEmpty(r.ModifiedTranscript) ? r.OriginalPrompt.Transcript : r.ModifiedTranscript,
        //        FileName = r.FileName,
        //        SpeechFilePath = appName + r.ContributorId + "/" + r.BlockId + "/modified/" + r.FileName
        //    });


        //    return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = recordingList });

        //}
        public async Task<IActionResult> DownloadSpeechFile(int recordingId, string location)
        {
            var recording = _recordingRepository.Find(r => r.Id == recordingId).FirstOrDefault();
            if (recording == null)
            {
                return Json(new { Success = false, Message = "Recording file is not found." });
            }
           
            var basePath = _configuration["AppSettings:UploadFileFolder"] + "\\" + recording.ContributorId + "\\" + recording.BlockId + "\\" + location;
            var fullFileName = recording.ContributorId + "_" + recording.OriginalPromptId + "_" + recording.BlockId + ".wav";
            var fullFilePath = Path.Combine(basePath, fullFileName);
           var fileName = recording.ContributorId + "_" + recording.OriginalPromptId + "_" + recording.BlockId + ".wav";

            var bytes = await System.IO.File.ReadAllBytesAsync(fullFilePath);
            return File(bytes, "audio/wav", fileName);
        }

      

        [HttpGet]
        public IActionResult UploadFile()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> UploadModifiedRecording(RecordingViewModel recordingVM)
        {
            var recording = _recordingRepository.Find(r => r.Id == recordingVM.Id && r.ContributorId == recordingVM.ContributorId)
                .FirstOrDefault();
            if (recording == null)
            {
                return Json(new { Success = true, Message = "Recording file is not found." });
            }
            //only for recordings that has blockID
            if (recording.BlockId != null && recording.BlockId > 0)
            {
                var recordingFile = Request.Form.Files[0];


                var fileExtension = Path.GetExtension(recordingFile.FileName);
                if (!string.IsNullOrEmpty(fileExtension) && fileExtension.ToLower() != ".wav")
                {
                    return Json(new { Success = false, Message = "Only WAV for recording file." });
                }
                var basePath = _configuration["AppSettings:UploadFileFolder"] + "\\" + recording.ContributorId + "\\" + recording.BlockId + "\\modified";
                var fullFileName = recording.ContributorId + "_" + recording.OriginalPromptId + "_" + recording.BlockId + ".wav";
                var fullFilePath = Path.Combine(basePath, fullFileName);
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }
                //delete recording if exists
                if (System.IO.File.Exists(fullFilePath))
                {
                    System.IO.File.Delete(fullFilePath);
                }

                try
                {
                    using (var stream = new FileStream(fullFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                    {
                        await recordingFile.CopyToAsync(stream);
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { Success = false, Message = "Error saving recording file to server. " + ex.Message });
                }

            }

            return Json(new { Success = true, Message = "File Uploaded Successfully." });

        }

        //media player method
        //public void PlayAudio()
        //{
        //    var player = new M.SoundPlayer();

        //    player.Load(new Uri("example.mp3", UriKind.Relative));

        //    player.Play();
        //}
    }
}
