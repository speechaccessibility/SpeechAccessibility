using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SpeechAccessibility.Annotator.Extensions;
using SpeechAccessibility.Annotator.Models;
using SpeechAccessibility.Annotator.Services;
using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Controllers
{
    [Authorize(Policy = "AllAnnotator")]
    public class ContributorController : Controller
    {
        private readonly IContributorRepository _contributorRepository;
        private readonly IContributorAssignedAnnotatorRepository _contributorAssignedAnnotatorRepository;
        private  readonly IContributorAssignedBlockRepository _contributorAssignedBlockRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IRecordingRepository _recordingRepository;
        public ContributorController(IContributorRepository contributorRepository, IContributorAssignedAnnotatorRepository contributorAssignedAnnotatorRepository, IUserRepository userRepository, IConfiguration configuration, IContributorAssignedBlockRepository contributorAssignedBlockRepository, IRecordingRepository recordingRepository)
        {
            _contributorRepository = contributorRepository;
            _contributorAssignedAnnotatorRepository = contributorAssignedAnnotatorRepository;
            _userRepository = userRepository;
            _configuration = configuration;
            _contributorAssignedBlockRepository = contributorAssignedBlockRepository;
            _recordingRepository = recordingRepository;
        }

        //[Authorize(Policy = "SLPAnnotatorAndTextAnnotatorAdmin")]
        public IActionResult Index()
        {
            return View();
        }
        //[Authorize(Policy = "SLPAnnotatorAndTextAnnotatorAdmin")]
        public IActionResult ContributorsWaitingForApproval()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoadContributorsWithSITFiles()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            //select only contributors that has SIT recordings
            Guid[] SITRecordingContributorIdList = _recordingRepository.Find(r=>r.StatusId==1 && r.BlockId==null).Select(c => c.ContributorId).ToArray();

           
            var contributors = _contributorRepository.Find(c => SITRecordingContributorIdList.Contains(c.Id))
                .Where(c => c.Id.ToString().Contains(searchValue) || c.FirstName.Contains(searchValue) || c.LastName.Contains(searchValue));
            var recordsTotal = contributors.Count();
            contributors = DynamicSortingExtensions<Contributor>.SetOrderByDynamic(contributors, Request.Form);
            contributors = contributors.Skip(skip).Take(pageSize);
            

            return Json(new { draw, data = contributors, recordsFiltered = recordsTotal, recordsTotal = recordsTotal });

        }



        //[Authorize(Policy = "SLPAnnotatorAndTextAnnotatorAdmin")]
        [HttpPost]
        public ActionResult LoadContributorsForApproval()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault());
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault());
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
           
            var contributors = _contributorRepository.Find(c => c.StatusId == 1)
                .Where(c => c.FirstName.Contains(searchValue) || c.LastName.Contains(searchValue) || c.Id.ToString().Contains(searchValue));

            var recordsTotal = contributors.Count();
            
            contributors = DynamicSortingExtensions<Contributor>.SetOrderByDynamic(contributors, Request.Form);
            contributors = contributors.Skip(skip).Take(pageSize);

            return Json(new { draw, data = contributors, recordsFiltered = recordsTotal, recordsTotal = recordsTotal });
        }

        //[Authorize(Policy = "SLPAnnotatorAndTextAnnotatorAdmin")]
        public IActionResult DeniedContributors()
        {
            return View();
        }

        //[Authorize(Policy = "SLPAnnotatorAndTextAnnotatorAdmin")]
        [HttpPost]
        public ActionResult LoadApprovedDeniedContributors(int filter)
        {

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault());
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault());
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            //only display Contributors that are assigned to logged in annotator
            //get assgined contributor list for annotator
            IQueryable<Contributor> contributors = null;
            IQueryable<Contributor> test = null;
            var currentUser = _userRepository.Find(u => u.NetId == User.Identity.Name).FirstOrDefault();
            var userRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (filter == 2 && userRole == "TextAnnotator") //approved contributors
            {
                Guid[] annotatorAssignedContributorIdList = _contributorAssignedAnnotatorRepository.Find(c => c.UserId == currentUser.Id)
                        .Select(u => u.ContributorId).ToArray();

                contributors = _contributorRepository.Find(c => c.StatusId == 2 && annotatorAssignedContributorIdList.Contains(c.Id))
                        .Where(c => c.FirstName.Contains(searchValue) || c.LastName.Contains(searchValue) || c.Id.ToString().Contains(searchValue));
            }
            else
            {
                contributors = _contributorRepository.Find(c => c.StatusId == filter)
                    .Where(c => c.FirstName.Contains(searchValue) || c.LastName.Contains(searchValue) || c.Id.ToString().Contains(searchValue));
            }
            var recordsTotal = contributors.Count();

            List<ApprovedDeniedContributorViewModel> contributorViewModels =
                new List<ApprovedDeniedContributorViewModel>();


            foreach (var contributor in contributors)
            {
                var numberAssignedBlocks = _contributorAssignedBlockRepository.Find(b => b.ContributorId == contributor.Id);
                var lastRecording = _recordingRepository.Find(r => r.ContributorId == contributor.Id && r.BlockId != null)
                    .OrderByDescending(r => r.CreateTS).FirstOrDefault();

                var contributorVM = new ApprovedDeniedContributorViewModel
                {
                    Contributor = contributor,
                    NumberAssignBlocks = numberAssignedBlocks.Any() ? numberAssignedBlocks.Count() : 0
                };
                if (lastRecording != null)
                    contributorVM.LastRecording = lastRecording.CreateTS.ToShortDateString();

                contributorViewModels.Add(contributorVM);
            }

            var wrk = DynamicSortingExtensions<ApprovedDeniedContributorViewModel>.SetOrderByDynamic(contributorViewModels.AsQueryable(), Request.Form);
            wrk = wrk.Skip(skip).Take(pageSize);

            return Json(new { draw, data = wrk, recordsFiltered = recordsTotal, recordsTotal = recordsTotal });

            //foreach (var contributor in contributors)
            //{
            //    var numberAssignedBlocks = _contributorAssignedBlockRepository.Find(b => b.ContributorId == contributor.Id);
            //    var lastRecording = _recordingRepository.Find(r => r.ContributorId == contributor.Id && r.BlockId != null)
            //        .OrderByDescending(r => r.CreateTS).FirstOrDefault();

            //    var contributorVM = new ApprovedDeniedContributorViewModelNew
            //    {
            //        LastName = contributor.LastName,
            //        FirstName = contributor.FirstName,
            //        MiddleName = contributor.MiddleName,
            //        Comments = contributor.Comments,
            //        Id = contributor.Id,
            //        NumberAssignBlocks = numberAssignedBlocks.Any() ? numberAssignedBlocks.Count() : 0
            //    };
            //    if (lastRecording != null)
            //        contributorVM.LastRecording = lastRecording.CreateTS.ToShortDateString();

            //    contributorViewModels.Add(contributorVM);
            //}

            //var wrk = DynamicSortingExtensions<ApprovedDeniedContributorViewModelNew>.SetOrderByDynamic(contributorViewModels.AsQueryable(), Request.Form);
            //wrk = wrk.Skip(skip).Take(pageSize);

            //return Json(new { draw, data = wrk, recordsFiltered = recordsTotal, recordsTotal = recordsTotal });

        }


        [Authorize(Policy = "TextAnnotatorAdmin")]
        public ActionResult GetAssignContributorAnnotators(Guid contributorId)
        {
            var contributor = _contributorRepository.Find(c => c.Id == contributorId).FirstOrDefault();
            ViewBag.ContributorId = contributor.Id;
            ViewBag.ContributorLastName = contributor.LastName;
            ViewBag.ContributorFirstName = contributor.FirstName;

            var assignedAnnotators = _contributorAssignedAnnotatorRepository.Find(a => a.ContributorId == contributorId)
                .Include(a=>a.User)
                .ToList();
           

            ContributorAssignedAnnotatorViewModel assignedAnnotatorVM = new ContributorAssignedAnnotatorViewModel
                {
                    Contributor = contributor,
                    ContributorAssignedAnnotators = assignedAnnotators,
                    ExistingAnnotators = _userRepository.Find(u=>u.RoleId==4 & u.Active=="Yes").OrderBy(r => r.LastName).Select(u =>
                            new SelectListItem()
                            {
                                Value = u.Id.ToString(),
                                Text = u.LastName + ", " + u.FirstName
                            })
                        .ToList()
                };

          
            return PartialView("_ViewContributorAssignedAnnotators", assignedAnnotatorVM);
        }

        [HttpPost]
        [Authorize(Policy = "TextAnnotatorAdmin")]
        public ActionResult AssignContributorAnnotator(Guid contributorId, int userId)
        {
            var annotator = _userRepository.Find(u => u.Id == userId).FirstOrDefault();

            ContributorAssignedAnnotator assignedAnnotator = new ContributorAssignedAnnotator
            {
                ContributorId = contributorId,
                UserId = userId
            };
            _contributorAssignedAnnotatorRepository.Insert(assignedAnnotator);

            return Json(new { Success = true, Message = new[] { new { id = assignedAnnotator.Id, annotator = annotator } } });
            
        }

        [HttpPost]
        [Authorize(Policy = "TextAnnotatorAdmin")]
        public ActionResult RemoveContributorAnnotator(int assignAnnotatorId)
        {
            var assignedAnnotator = _contributorAssignedAnnotatorRepository.Find(a => a.Id == assignAnnotatorId)
                .FirstOrDefault();

            _contributorAssignedAnnotatorRepository.Delete(assignedAnnotator);

            var existingAnnotators = string.Join(",",
                _contributorAssignedAnnotatorRepository.Find(c => c.ContributorId == assignedAnnotator.ContributorId)
                    .Select(a => a.User.Id));

            return Json(new { Success = true, Message = existingAnnotators });

        }


        [Authorize(Policy = "SLPAnnotator")]
        [HttpPost]
        public async Task<ActionResult> ApproveDenyContributor(Guid contributorId, string comment,string passwordChange, int action)
        {
            var contributor = _contributorRepository.Find(c => c.Id == contributorId).FirstOrDefault();
            if (contributor != null)
            {
                contributor.ChangePassword = passwordChange == "Yes";
                contributor.StatusId = action;
                contributor.Comments = comment;
                _contributorRepository.Update(contributor);
            }
            //todo: need to send email for approval and deny here
            StringBuilder message = new StringBuilder();
            var emailSubject = "";
            if (action == 2) //approve
            {
                message.Append("Dear " + contributor.FirstName);
                message.Append("<br>Thank you so much for your interest in participating in the Speech Accessibility Project.");
                message.Append("<br>We are delighted to share that you’ve been accepted to participate. ");
                message.Append("Please watch for an email with more information from your LSVT Global mentor to guide you through the participation process.");
                if (passwordChange == "Yes")
                {
                    message.Append("<br>");
                    message.Append("You are required to change your password when you login in.");
                }

                message.Append("Thank you for sharing your voice!");

                message.Append("<br>If you have any questions, please contact speechaccessibility@beckman.illinois.edu.");
                message.Append("<br>Sincerely,");
                message.Append("<br>The Speech Accessibility Project Team");
                message.Append("<br>University of Illinois Urbana - Champaign");

               
                emailSubject = "Your registration has been approved.";

            }
            else if (action == 3)//deny
            {
                message.Append("Dear " + contributor.FirstName);
                message.Append("<br>Thank you so much for your interest in participating in the Speech Accessibility Project.");
                message.Append("<br>Unfortunately, you do not meet the current criteria for the project. If you have specific questions about this,");
                message.Append("please contact speechaccessibility@beckman.illinois.edu.");
                message.Append("<br>Sincerely,");
                message.Append("<br>The Speech Accessibility Project Team");
                message.Append("<br>University of Illinois Urbana - Champaign");

                emailSubject = "Your registration has been denied.";


            }
            
            var error = await SendEmailToContributor(contributor, emailSubject, message, _configuration["AppSettings:EmailServer"]);
            return Json(!string.IsNullOrEmpty(error) ? new { Success = false, Message = error } : new { Success = true, Message = "" });
        }

        //[Authorize(Policy = "AllAnnotator")]
        [HttpPost]
        public ActionResult LoadApprovedContributors()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var contributors = _contributorRepository.Find(c=>c.StatusId==2)
                .Where(c=>c.Id.ToString().Contains(searchValue) || c.FirstName.Contains(searchValue)||c.LastName.Contains(searchValue));
            var recordsTotal = contributors.Count();
            contributors = DynamicSortingExtensions<Contributor>.SetOrderByDynamic(contributors, Request.Form);
            contributors = contributors.Skip(skip).Take(pageSize);
           
            
            return Json(new { draw, data = contributors, recordsFiltered = recordsTotal, recordsTotal = recordsTotal });

        }


        private async Task<string> SendEmailToContributor(Contributor contributor, string emailSubject, StringBuilder emailContent, string emailServer)
        {
            string toEmailAddress;
            string subject;
            StringBuilder body = new StringBuilder();
            var fromEmailAddress = _configuration["AppSettings:SpeechAccessibilityTeamEmail"];
            var emailService = new EmailService();
            var actualToEmailAddress = contributor.EmailAddress;
            string[] toEmails;
            if (_configuration["AppSettings:DeveloperMode"] == "Yes")
            {
                toEmailAddress = _configuration["AppSettings:TestingEmail"];
                subject = "Testing: Your registration has been approved.";
                body.Append("This email was sent in a testing mode.The actual email should be sent to " + actualToEmailAddress +
                            ".<br>");

                toEmails = new[] { toEmailAddress };
            }
            else if (_configuration["AppSettings:TestingMode"] == "Yes")
            {
                toEmailAddress = User.Identity.Name + "@illinois.edu";
                subject = "Testing: " + emailSubject;
                body.Append("This email was sent in a testing mode.The actual email should be sent to " + actualToEmailAddress +
                            ".<br>");
                toEmails = new[] { toEmailAddress };
            }
            else
            {
                toEmailAddress = actualToEmailAddress;
                subject = emailSubject;
                toEmails = new[] { toEmailAddress};
            }


            body.Append(emailContent);
           
            var error = await emailService.SendEmail(fromEmailAddress, toEmails, null, null, subject, body,
                emailServer);
            return error;
        }

    }
}
