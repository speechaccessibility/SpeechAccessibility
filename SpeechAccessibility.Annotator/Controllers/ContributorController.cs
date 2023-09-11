using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using SpeechAccessibility.Annotator.Extensions;
using SpeechAccessibility.Annotator.Models;
using SpeechAccessibility.Annotator.Services;
using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Controllers
{
    [Authorize(Policy = "AllAnnotatorAndExternalSLPAnnotator")]
    public class ContributorController : Controller
    {
        private readonly IContributorRepository _contributorRepository;
        private readonly IContributorAssignedAnnotatorRepository _contributorAssignedAnnotatorRepository;
        private  readonly IContributorAssignedBlockRepository _contributorAssignedBlockRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IRecordingRepository _recordingRepository;
        private  readonly  IContributorFollowUpRepository _contributorFollowUpRepository;
        private readonly IEmailLoggingRepository _emailLoggingRepository;
        private readonly IUserSubRoleRepository _userSubRoleRepository;
        private readonly ISubRoleRepository _subRoleRepository;
        private readonly IContributorSubStatusRepository _contributorSubStatusRepository;
        public ContributorController(IContributorRepository contributorRepository, IContributorAssignedAnnotatorRepository contributorAssignedAnnotatorRepository, IUserRepository userRepository, IConfiguration configuration, IContributorAssignedBlockRepository contributorAssignedBlockRepository, IRecordingRepository recordingRepository, IContributorFollowUpRepository contributorFollowUpRepository, IEmailLoggingRepository emailLoggingRepository, IUserSubRoleRepository userSubRoleRepository, ISubRoleRepository subRoleRepository, IContributorSubStatusRepository contributorSubStatusRepository)
        {
            _contributorRepository = contributorRepository;
            _contributorAssignedAnnotatorRepository = contributorAssignedAnnotatorRepository;
            _userRepository = userRepository;
            _configuration = configuration;
            _contributorAssignedBlockRepository = contributorAssignedBlockRepository;
            _recordingRepository = recordingRepository;
            _contributorFollowUpRepository = contributorFollowUpRepository;
            _emailLoggingRepository = emailLoggingRepository;
            _userSubRoleRepository = userSubRoleRepository;
            _subRoleRepository = subRoleRepository;
            _contributorSubStatusRepository = contributorSubStatusRepository;
        }

        //[Authorize(Policy = "SLPAnnotatorAndTextAnnotatorAdmin")]
        //this action is for Approved Contributor page
        public IActionResult Index(int etiologyId)
        {
            //if external SLP annotator, check to make sure the role is matched
            var hasSubRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.OtherPhone)?.Value;
            if (hasSubRole == "Yes")
            {
                if (UtilsExtension.IsMatchedRole(_userSubRoleRepository, etiologyId, User.Identity.Name) == false)
                    return RedirectToRoute(new { controller = "Home", action = "Error" });

            }

            var subRoleName = _subRoleRepository.Find(s => s.EtiologyId == etiologyId).Include(e => e.Etiology).FirstOrDefault();

            ViewBag.SubRole = etiologyId;
            if (subRoleName != null) ViewBag.SubRoleName = subRoleName.Etiology.Name;
            //ViewBag.SubStatus = _contributorSubStatusRepository.Find(s => s.StatusId == 2).OrderBy(s=>s.DisplayOrder).Select(a => new SelectListItem()
            //    {
            //        Value = a.Id.ToString(),
            //        Text = a.Name
            //    })
            //    .ToList();
            ViewBag.SubStatus = GetSubStatus();
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
                    .Include(c => c.Etiology)
                    .Where(c => c.Id.ToString().Contains(searchValue) || c.FirstName.Contains(searchValue) || c.LastName.Contains(searchValue) || c.Etiology.Name.Contains(searchValue));
            var recordsTotal = contributors.Count();
            contributors = DynamicSortingExtensions<Contributor>.SetOrderByDynamic(contributors, Request.Form);
            contributors = contributors.Skip(skip).Take(pageSize);
            

            return Json(new { draw, data = contributors, recordsFiltered = recordsTotal, recordsTotal = recordsTotal });

        }

        //[Authorize(Policy = "SLPAnnotatorAndTextAnnotatorAdmin")]
        public IActionResult ContributorsWaitingForApproval(int etiologyId)
        {
            //if external SLP annotator, check to make sure the role is matched
            var hasSubRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.OtherPhone)?.Value;
            if (hasSubRole == "Yes")
            {
                if (UtilsExtension.IsMatchedRole(_userSubRoleRepository, etiologyId, User.Identity.Name) == false)
                    return RedirectToRoute(new { controller = "Home", action = "Error" });

            }
            var subRoleName = _subRoleRepository.Find(s=>s.EtiologyId== etiologyId).Include(e=>e.Etiology).FirstOrDefault();
            ViewBag.SubRole = etiologyId;
            if (subRoleName != null) ViewBag.SubRoleName = subRoleName.Etiology.Name;
            ViewBag.SubStatus = GetSubStatus();
            return View();
        }

        //[Authorize(Policy = "SLPAnnotatorAndTextAnnotatorAdmin")]
        [HttpPost]
        public ActionResult LoadContributorsForApproval(int etiologyId)
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault());
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault());
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
           
            var contributors = _contributorRepository.Find(c => c.StatusId == 1 && c.EtiologyId== etiologyId)
                .Where(c => c.FirstName.Contains(searchValue) || c.LastName.Contains(searchValue) || c.Id.ToString().Contains(searchValue)
                            || c.EmailAddress.Contains(searchValue) || c.HelperEmail.Contains(searchValue));

            var recordsTotal = contributors.Count();
            
            contributors = DynamicSortingExtensions<Contributor>.SetOrderByDynamic(contributors, Request.Form);
            contributors = contributors.Skip(skip).Take(pageSize);

            return Json(new { draw, data = contributors, recordsFiltered = recordsTotal, recordsTotal = recordsTotal });
        }

        //[Authorize(Policy = "SLPAnnotatorAndTextAnnotatorAdmin")]
        public IActionResult DeniedContributors(int etiologyId)
        {
            //if external SLP annotator, check to make sure the role is matched
            var hasSubRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.OtherPhone)?.Value;
            if (hasSubRole == "Yes")
            {
                if (UtilsExtension.IsMatchedRole(_userSubRoleRepository, etiologyId, User.Identity.Name) == false)
                    return RedirectToRoute(new { controller = "Home", action = "Error" });

            }
            var subRoleName = _subRoleRepository.Find(s => s.EtiologyId == etiologyId).Include(e => e.Etiology).FirstOrDefault();

            ViewBag.SubRole = etiologyId;
            if (subRoleName != null) ViewBag.SubRoleName = subRoleName.Etiology.Name;
            return View();
        }

        public IActionResult NonResponsiveContributors(int etiologyId)
        {
            //if external SLP annotator, check to make sure the role is matched
            var hasSubRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.OtherPhone)?.Value;
            if (hasSubRole == "Yes")
            {
                if (UtilsExtension.IsMatchedRole(_userSubRoleRepository, etiologyId, User.Identity.Name) == false)
                    return RedirectToRoute(new { controller = "Home", action = "Error" });

            }
            var subRoleName = _subRoleRepository.Find(s => s.EtiologyId == etiologyId).Include(e => e.Etiology).FirstOrDefault();

            ViewBag.SubRole = etiologyId;
            if (subRoleName != null) ViewBag.SubRoleName = subRoleName.Etiology.Name;
            return View();
        }

        //[Authorize(Policy = "SLPAnnotatorAndTextAnnotatorAdmin")]
        [HttpPost]
        public ActionResult LoadContributors(int filter, int subRole)
        {

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault());
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault());
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            //only display Contributors that are assigned to logged in annotator
            //get assigned contributor list for annotator
            IQueryable<Contributor> contributors = null;
            var currentUser = _userRepository.Find(u => u.NetId == User.Identity.Name).FirstOrDefault();
            var userRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (filter == 2 && userRole == "TextAnnotator") //approved contributors
            {
                Guid[] annotatorAssignedContributorIdList = _contributorAssignedAnnotatorRepository.Find(c => c.UserId == currentUser.Id)
                        .Select(u => u.ContributorId).ToArray();

                contributors = _contributorRepository.Find(c => c.StatusId == 2 && c.EtiologyId == subRole && annotatorAssignedContributorIdList.Contains(c.Id))
                        .Include(s=>s.ContributorSubStatus).Include(c=>c.ContributorDetails)
                        .Where(c => c.FirstName.Contains(searchValue) || c.LastName.Contains(searchValue) || c.Id.ToString().Contains(searchValue) || c.Comments.Contains(searchValue));
            }
            else
            {
                contributors = _contributorRepository.Find(c =>  c.EtiologyId == subRole && c.StatusId == filter )
                    .Include(s => s.ContributorSubStatus).Include(c=>c.ContributorDetails)
                    .Where(c => c.FirstName.Contains(searchValue) || c.LastName.Contains(searchValue) || c.Id.ToString().Contains(searchValue) || c.Comments.Contains(searchValue));
            }
            var recordsTotal = contributors.Count();

            List<ApprovedDeniedContributorViewModel> contributorViewModels =
                new List<ApprovedDeniedContributorViewModel>();


            foreach (var contributor in contributors)
            {
                var numberAssignedBlocks = _contributorAssignedBlockRepository.Find(b => b.ContributorId == contributor.Id);
                var lastRecording = _recordingRepository.Find(r => r.ContributorId == contributor.Id && r.BlockId != null)
                    .OrderByDescending(r => r.CreateTS).FirstOrDefault();
                var numberAssignedAnnotator = _contributorAssignedAnnotatorRepository.Find(b => b.ContributorId == contributor.Id);

                var contributorVM = new ApprovedDeniedContributorViewModel
                {
                    Contributor = contributor,
                    NumberAssignBlocks = numberAssignedBlocks.Any() ? numberAssignedBlocks.Count() : 0,
                    ApprovedStatus = contributor.ContributorSubStatus != null ? contributor.ContributorSubStatus.Name : ""
                };
               
                if (lastRecording != null)
                    contributorVM.LastRecording = lastRecording.CreateTS;
                else
                    contributorVM.LastRecording = null;
                contributorVM.AnnotatorAssigned = numberAssignedAnnotator.Any() ? "Yes" : "No";

                if (filter is 2 or 4) //get the follow-up dates
                {
                    contributorVM.FollowUpDate = String.Join("; ", _contributorFollowUpRepository.Find(c => c.ContributorId == contributor.Id)
                        .Select(f => f.SendTS));
                }
                contributorViewModels.Add(contributorVM);
            }

            var wrk = DynamicSortingExtensions<ApprovedDeniedContributorViewModel>.SetOrderByDynamic(contributorViewModels.AsQueryable(), Request.Form);
            wrk = wrk.Skip(skip).Take(pageSize);

            return Json(new { draw, data = wrk, recordsFiltered = recordsTotal, recordsTotal = recordsTotal });

         
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

        [Authorize(Policy = "SLPAnnotatorAndExternalSLPAnnotator")]
        [HttpPost]
        public async Task<ActionResult> UpdateContributor(Guid contributorId, string comment, string passwordChange, int subRole, int action)
        {
            //make sure the External annotator has permission for this subrole
            var hasSubRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.OtherPhone)?.Value;
            if (hasSubRole == "Yes")
            {
                if (UtilsExtension.IsMatchedRole(_userSubRoleRepository, subRole, User.Identity.Name) == false)
                    return Json(new { Success = false, Message = "You do not have permission for this contributor." });

            }

            var contributor = _contributorRepository.Find(c => c.Id == contributorId).FirstOrDefault();
            if (contributor == null)
            {
                return Json(new { Success = false, Message = "Contributor is not found." });
            }

           
            //if (action==2)
            //    contributor.ChangePassword = passwordChange == "Yes";
            contributor.StatusId = action;
            contributor.Comments = comment;

            if (action == 2) //approve
            {
                contributor.ChangePassword = passwordChange == "Yes";
                contributor.ApproveTS = DateTime.Now;
                contributor.SubStatusId = 3; //set to Not-Started
            }
            else
                contributor.UpdateTS = DateTime.Now;

            contributor.ApproveDenyBy = User.Identity.Name;
            _contributorRepository.Update(contributor);
          

            StringBuilder message = new StringBuilder();
            var emailSubject = "";
            var error = "";
            if (action == 2) //approve
            {
                //check for ExternalSLPAnnotator
                //if ParkinsonsInd, this contributor was routed to ExternalSLPAnnotator Group
                message.Append("Dear " + contributor.FirstName);
                message.Append("<br>Thank you so much for your interest in participating in the Speech Accessibility Project.");
                message.Append("<br>We are delighted to share that you’ve been accepted to participate. ");
                if (contributor.EtiologyId == 1) // for Parkinson's Disease
                {
                    message.Append("Please watch for an email with more information from your LSVT Global mentor to guide you through the participation process.");
                }
                else
                {
                    message.Append("Please visit the Speech Accessibility App at " + _configuration["AppSettings:ContributorWebLink"] + ". ");
                    message.Append("Log in and click 'Record Prompt.' You should automatically see our informed consent form. Once you consent, you will be prompted to begin recording.");

                }

                if (passwordChange == "Yes")
                {
                    message.Append("<br>");
                    message.Append("You are required to change your password when you login in.");
                }

                message.Append("<br>Thank you for sharing your voice! ");
                message.Append("If you have any questions, please contact " + _configuration["AppSettings:SpeechAccessibilityTeamEmail"] + ".");
                message.Append("<br><br>Sincerely,");
                message.Append("<br>The Speech Accessibility Project Team");
                message.Append("<br>University of Illinois Urbana - Champaign");

                emailSubject = "Your registration has been approved.";

                error = await SendEmailToContributor(contributor, emailSubject, message, _configuration["AppSettings:EmailServer"]);
            }
            else if (action == 3)//deny
            {
                message.Append("Dear " + contributor.FirstName);
                message.Append("<br>Thank you so much for your interest in participating in the Speech Accessibility Project.");
                message.Append("<br>Unfortunately, you do not meet the current criteria for the project. If you have specific questions about this,");
                message.Append("please contact " + _configuration["AppSettings:SpeechAccessibilityTeamEmail"] + ".");
                message.Append("<br>Sincerely,");
                message.Append("<br>The Speech Accessibility Project Team");
                message.Append("<br>University of Illinois Urbana - Champaign");

                emailSubject = "Your registration has been denied.";

                error = await SendEmailToContributor(contributor, emailSubject, message, _configuration["AppSettings:EmailServer"]);

            }

            //logging the email
            if (action is 2 or 3)
            {
                var emailLogging = new EmailLogging
                {
                    ContributorId = contributor.Id,
                    Subject = emailSubject,
                    SendTo = contributor.EmailAddress,
                    Message = message.ToString(),
                    Error = error,
                    SendTS = DateTime.Now,
                    SendBy = User.Identity.Name
                };
                _emailLoggingRepository.Insert(emailLogging);
            }
           

            return Json(!string.IsNullOrEmpty(error) ? new { Success = false, Message = error } : new { Success = true, Message = "" });
        }

       

        [Authorize(Policy = "SLPAnnotatorAndExternalSLPAnnotator")]
        [HttpPost]
        public async Task<ActionResult> SendFollowUpToContributor(Guid contributorId, string message,int subRole)
        {
            //make sure the External annotator has permission for this subrole
            var hasSubRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.OtherPhone)?.Value;
            if (hasSubRole == "Yes")
            {
                if (UtilsExtension.IsMatchedRole(_userSubRoleRepository, subRole, User.Identity.Name) == false)
                    return Json(new { Success = false, Message = "You do not have permission for this contributor." });

            }

            var contributor = _contributorRepository.Find(c => c.Id == contributorId).FirstOrDefault();
            if (contributor == null)
            {
                return Json(new { Success = false, Message = "Contributor is not found." });
            }
            
            StringBuilder followUpMessage = new StringBuilder();
            var emailSubject = "Speech Accessibility Project - Follow-up ";
            var error = "";
            message = message.Replace("\n", "<br />");
            followUpMessage.Append("<br>" + message);
           
           
            error = await SendEmailToContributor(contributor, emailSubject, followUpMessage, _configuration["AppSettings:EmailServer"]);
            //need to save this email to the database
            if (string.IsNullOrEmpty(error))
            {
                var followUp = new ContributorFollowUp
                {
                    ContributorId = contributorId,
                    EmailContent = message,
                    SendTS = DateTime.Now,
                    SendBy = User.Identity.Name
                };
                _contributorFollowUpRepository.Insert(followUp);
               
            }

            //logging the email
            var emailLogging = new EmailLogging
            {
                ContributorId = contributor.Id,
                Subject = emailSubject,
                SendTo = contributor.EmailAddress,
                Message = followUpMessage.ToString(),
                Error = error,
                SendTS = DateTime.Now,
                SendBy = User.Identity.Name
            };
            _emailLoggingRepository.Insert(emailLogging);

            return Json(!string.IsNullOrEmpty(error) ? new { Success = false, Message = error } : new { Success = true, Message = contributor.EmailAddress });
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
                .Include(c=>c.Etiology)
                .Where(c=>c.Id.ToString().Contains(searchValue) || c.FirstName.Contains(searchValue)||c.LastName.Contains(searchValue) || c.Etiology.Name.Contains(searchValue));
            var recordsTotal = contributors.Count();
            contributors = DynamicSortingExtensions<Contributor>.SetOrderByDynamic(contributors, Request.Form);
            contributors = contributors.Skip(skip).Take(pageSize);
           
            
            return Json(new { draw, data = contributors, recordsFiltered = recordsTotal, recordsTotal = recordsTotal });

        }

        [Authorize(Policy = "AllAnnotatorAndExternalSLPAnnotator")]
        [HttpPost]
        public async Task<ActionResult> EditContributorInfo(Guid contributorId,string contributorEmail, string helperEmail,string birthYear,int subStatusId, int subRole, string comments, string helperPhone)
        {
            //make sure the External annotator has permission for this subrole
            var hasSubRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.OtherPhone)?.Value;
            if (hasSubRole == "Yes")
            {
                if (UtilsExtension.IsMatchedRole(_userSubRoleRepository, subRole, User.Identity.Name) == false)
                    return Json(new { Success = false, Message = "You do not have permission for this contributor." });

            }

            var contributor = _contributorRepository.Find(c => c.Id == contributorId).FirstOrDefault();
            if (contributor == null)
            {
                return Json(new { Success = false, Message = "Contributor is not found." });
            }

            contributor.EmailAddress = contributorEmail;
            contributor.HelperEmail = helperEmail;
            contributor.HelperPhoneNumber = helperPhone;
            //if (!string.IsNullOrEmpty(birthYear))
            contributor.BirthYear = birthYear;
            if(subStatusId>0)
                contributor.SubStatusId= subStatusId;
            if (!string.IsNullOrEmpty(comments))
                contributor.Comments = comments;
            contributor.UpdateTS = DateTime.Now;
            contributor.ApproveDenyBy = User.Identity.Name;
            _contributorRepository.Update(contributor);

            return Json(new { Success = true, Message = "updated" });

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
                subject = "Testing: " + emailSubject;
                body.Append("This email was sent in a testing mode. The actual email should be sent to " + actualToEmailAddress +
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

        [HttpPost]
        public IActionResult ExportNonResponsiveContributors(int subRole)
        {
            var hasSubRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.OtherPhone)?.Value;
            if (hasSubRole == "Yes")
            {
                if (UtilsExtension.IsMatchedRole(_userSubRoleRepository, subRole, User.Identity.Name) == false)
                    return RedirectToRoute(new { controller = "Home", action = "Error" });

            }

            
          
            var contributors = _contributorRepository.Find(c => c.EtiologyId == subRole && c.StatusId == 4)
                 .Select(c => new { c.FirstName, c.LastName, c.EmailAddress,c.HelperFirstName,c.HelperLastName, c.HelperEmail }).ToList();

             using (XLWorkbook wb = new XLWorkbook())
             {
                 wb.Worksheets.Add(UtilsExtension.ToDataTable(contributors));
                 using (MemoryStream stream = new MemoryStream())
                 {
                     wb.SaveAs(stream);
                     return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "NonResponsive.xlsx");
                 }
             }
        }

      
        private List<SelectListItem> GetSubStatus()
        {
           return _contributorSubStatusRepository.Find(s => s.StatusId == 2).OrderBy(s => s.DisplayOrder).Select(a => new SelectListItem()
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                })
                .ToList();
        }
    }
}
