using System;
using System.Collections.Generic;
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
        private readonly IContributorViewRepository _contributorViewRepository;
        private readonly IContributorAssignedAnnotatorRepository _contributorAssignedAnnotatorRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IRecordingRepository _recordingRepository;
        private readonly IContributorFollowUpRepository _contributorFollowUpRepository;
        private readonly IEmailLoggingRepository _emailLoggingRepository;
        private readonly IUserSubRoleRepository _userSubRoleRepository;
        private readonly ISubRoleRepository _subRoleRepository;
        private readonly IContributorSubStatusRepository _contributorSubStatusRepository;
        private readonly IEtiologyViewRepository _etiologyRepository;
        private readonly IApprovedDeniedContributorRepository _aprovedDeniedContributorRepository;
        private readonly IContributorRepository _contributorRepository;
        private readonly IRegisterLinkRepository _registerLinkRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IEtiologyContactEmailAddressRepository _etiologyContactEmailAddressRepository;
        private readonly IAspNetUsersRepository _usersRepository;

        public ContributorController(IContributorViewRepository contributorViewRepository, IContributorAssignedAnnotatorRepository contributorAssignedAnnotatorRepository
            , IUserRepository userRepository, IConfiguration configuration, IRecordingRepository recordingRepository, IContributorFollowUpRepository contributorFollowUpRepository
            , IEmailLoggingRepository emailLoggingRepository, IUserSubRoleRepository userSubRoleRepository, ISubRoleRepository subRoleRepository
            , IContributorSubStatusRepository contributorSubStatusRepository, IEtiologyViewRepository etiologyRepository
            , IApprovedDeniedContributorRepository aprovedDeniedContributorRepository, IContributorRepository contributorRepository, IRegisterLinkRepository registerLinkRepository
            , ICategoryRepository categoryRepository, IEtiologyContactEmailAddressRepository etiologyContactEmailAddressRepository, IAspNetUsersRepository usersRepository)
        {
            _contributorViewRepository = contributorViewRepository;
            _contributorAssignedAnnotatorRepository = contributorAssignedAnnotatorRepository;
            _userRepository = userRepository;
            _configuration = configuration;
            _recordingRepository = recordingRepository;
            _contributorFollowUpRepository = contributorFollowUpRepository;
            _emailLoggingRepository = emailLoggingRepository;
            _userSubRoleRepository = userSubRoleRepository;
            _subRoleRepository = subRoleRepository;
            _contributorSubStatusRepository = contributorSubStatusRepository;
            _etiologyRepository = etiologyRepository;
            _aprovedDeniedContributorRepository = aprovedDeniedContributorRepository;
            _contributorRepository = contributorRepository;
            _registerLinkRepository = registerLinkRepository;
            _categoryRepository = categoryRepository;
            _etiologyContactEmailAddressRepository = etiologyContactEmailAddressRepository;
            _usersRepository = usersRepository;
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

            ViewBag.SubRole = etiologyId;
            ViewBag.SubRoleName = _etiologyRepository.Find(e => e.Id == etiologyId).FirstOrDefault()?.Name;
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

            var contributors = _contributorViewRepository.Find(c => SITRecordingContributorIdList.Contains(c.Id))
               .Where(c => c.Id.ToString().Contains(searchValue) || c.FirstName.Contains(searchValue) || c.LastName.Contains(searchValue)
               || c.EtiologyName.Contains(searchValue));


            var recordsTotal = contributors.Count();
            contributors = DynamicSortingExtensions<ContributorView>.SetOrderByDynamic(contributors, Request.Form);
            contributors = contributors.Skip(skip).Take(pageSize);
            

            return Json(new { draw, data = contributors, recordsFiltered = recordsTotal, recordsTotal = recordsTotal });

        }

        [Authorize(Policy = "SLPAnnotatorAndTextAnnotatorAdminAndExternalSLPAnnotator")]
        public IActionResult ContributorsWaitingForApproval(int etiologyId)
        {

            //if external SLP annotator, check to make sure the role is matched
            var hasSubRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.OtherPhone)?.Value;
            if (hasSubRole == "Yes")
            {
                if (UtilsExtension.IsMatchedRole(_userSubRoleRepository, etiologyId, User.Identity.Name) == false)
                    return RedirectToRoute(new { controller = "Home", action = "Error" });

            }

            List<SelectListItem> promptCatForMentor = new SelectList(_categoryRepository.Find(c => c.Active == "Yes" && c.DisplayForMentor == "Yes").OrderBy(c=>c.Id), "Id", "Description").ToList();
            ViewBag.PromptCategories = promptCatForMentor;

            List<SelectListItem> etiologies = new SelectList(_etiologyRepository.Find(c => c.Active == "Yes" )
                .OrderBy(c => c.DisplayOrder), "Id", "Name").ToList();
            ViewBag.Etiologies = etiologies;


            ViewBag.SubRole = etiologyId;
            ViewBag.SubRoleName = _etiologyRepository.Find(e => e.Id == etiologyId).FirstOrDefault()?.Name;

            ViewBag.SubStatus = GetSubStatus();
            ViewBag.ContributorLink = _configuration["AppSettings:ContributorWebLink"];

            var registerLink = _registerLinkRepository.Find(l => l.EtiologyId == etiologyId).FirstOrDefault();
            ViewBag.RegisterLink = registerLink == null ? "" : registerLink.RegisterName;
          
            return View();
        }

        [Authorize(Policy = "SLPAnnotatorAndTextAnnotatorAdminAndExternalSLPAnnotator")]
        [HttpPost]
        public ActionResult LoadContributorsForApproval(int etiologyId)
        {
            ViewBag.ContributorLink = _configuration["AppSettings:ContributorWebLink"];
            var draw = Request.Form["draw"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault());
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault());
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            List<int> includeStatus = new List<int>();
            includeStatus.Add(1);
            includeStatus.Add(5);

            var contributors = _contributorViewRepository.Find(c => includeStatus.Contains(c.StatusId) && c.EtiologyId== etiologyId)
            .Where(c => c.FirstName.Contains(searchValue) || c.LastName.Contains(searchValue) || c.Id.ToString().Contains(searchValue)
                        || c.EmailAddress.Contains(searchValue) || c.HelperEmail.Contains(searchValue));
            
            var recordsTotal = contributors.Count();
            
            contributors = DynamicSortingExtensions<ContributorView>.SetOrderByDynamic(contributors, Request.Form);
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
           
            ViewBag.SubRole = etiologyId;
            ViewBag.SubRoleName = _etiologyRepository.Find(e => e.Id == etiologyId).FirstOrDefault()?.Name;

            List<SelectListItem> etiologies = new SelectList(_etiologyRepository.Find(c => c.Active == "Yes")
                .OrderBy(c => c.DisplayOrder), "Id", "Name").ToList();
            ViewBag.Etiologies = etiologies;

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
            //todo: use Etiology view here
            //var subRoleName = _subRoleRepository.Find(s => s.EtiologyId == etiologyId).Include(e => e.Etiology).FirstOrDefault();
            var subRoleName = _subRoleRepository.Find(s => s.EtiologyId == etiologyId).FirstOrDefault();

            ViewBag.SubRole = etiologyId;
            ViewBag.SubRoleName = _etiologyRepository.Find(e => e.Id == etiologyId).FirstOrDefault()?.Name;
            return View();
        }

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
            IQueryable<ApprovedDeniedContributor> contributors = null;
            var currentUser = _userRepository.Find(u => u.NetId == User.Identity.Name).FirstOrDefault();
            var userRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (filter == 2 && userRole == "TextAnnotator") //approved contributors
            {
                Guid[] annotatorAssignedContributorIdList = _contributorAssignedAnnotatorRepository.Find(c => c.UserId == currentUser.Id)
                        .Select(u => u.ContributorId).ToArray();
                contributors = _aprovedDeniedContributorRepository.Find(c => c.StatusId == filter && c.EtiologyId == subRole && annotatorAssignedContributorIdList.Contains(c.Id))
                    .Where(c => c.FirstName.Contains(searchValue) || c.LastName.Contains(searchValue) || c.Id.ToString().Contains(searchValue) || c.Comments.Contains(searchValue));

            }
            else
            {
               contributors = _aprovedDeniedContributorRepository.Find(c => c.StatusId == filter && c.EtiologyId == subRole )
                    .Where(c => c.FirstName.Contains(searchValue) || c.LastName.Contains(searchValue) ||
                                c.Id.ToString().Contains(searchValue) || c.Comments.Contains(searchValue));

            }
            var recordsTotal = contributors.Count();
            contributors = DynamicSortingExtensions<ApprovedDeniedContributor>.SetOrderByDynamic(contributors, Request.Form);
            contributors = contributors.Skip(skip).Take(pageSize);
            return Json(new { draw, data = contributors, recordsFiltered = recordsTotal, recordsTotal = recordsTotal });

        }

      

        [Authorize(Policy = "TextAnnotatorAdmin")]
        public ActionResult GetAssignContributorAnnotators(Guid contributorId)
        {
            var contributor = _contributorViewRepository.Find(c => c.Id == contributorId).FirstOrDefault();
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
        public async Task<ActionResult> UpdateContributor(Guid contributorId, string comment, string passwordChange, int subRole, int action, int promptCategory, int etiologyId)
        {
            //action: 1-un-deny or move Etiology; 2-approve;3-deny;
            var sendTeamForEtiologyChanged = false;
            var oldEtiologyName = "Other";
            //make sure the External annotator has permission for this subrole
            var hasSubRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.OtherPhone)?.Value;
            if (hasSubRole == "Yes")
            {
                if (UtilsExtension.IsMatchedRole(_userSubRoleRepository, subRole, User.Identity.Name) == false)
                    return Json(new { Success = false, Message = "You do not have permission for this contributor." });
            }

            var contributor = _contributorRepository.Find(c => c.Id == contributorId).Include(c=>c.Etiology).FirstOrDefault();
            
            if (contributor == null)
            {
                return Json(new { Success = false, Message = "Contributor is not found." });
            }

            if (action == 1) //un-deny and contributor is not registered
            {
                if (contributor.EtiologyId != etiologyId)
                {
                    oldEtiologyName = contributor.Etiology.Name;
                    sendTeamForEtiologyChanged = true;
                }
                contributor.EtiologyId = etiologyId;
                
                contributor.StatusId = string.IsNullOrEmpty(contributor.IdentityUserId) ? 5 : action; //un-deny and contributor is not registered if contributor.IdentityUserId is null
            }
            else
            {
                contributor.StatusId = action;
            }

            contributor.Comments = comment;

            if (action == 2) //approve
            {
                contributor.ChangePassword = passwordChange == "Yes";
                contributor.ApproveTS = DateTime.Now;
                contributor.SubStatusId = 3; //set to Not-Started
                //only change Contributor's Etioglogy for Other 
                if (contributor.EtiologyId == 5)
                {
                    if(contributor.EtiologyId != etiologyId)
                    {
                        contributor.EtiologyId = etiologyId;
                        //send email the the team in charge for the new Etiology
                        sendTeamForEtiologyChanged = true;
                    }

                }
                //contributor.EtiologyId = etiologyId;
                if ((etiologyId == 3 || etiologyId == 4 || etiologyId == 6) && promptCategory > 0) // only change prompt for Cerebral Palsy, Stroke and ALS
                    contributor.PromptCategoryId = promptCategory;
            }
            else
                contributor.UpdateTS = DateTime.Now;

            contributor.ApproveDenyBy = User.Identity.Name;
            _contributorRepository.Update(contributor);
          

            StringBuilder message = new StringBuilder();
            var emailSubject = "";
            var error = "";
            //sending email
            if (action == 2 && subRole !=2) //approve
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

                message.Append("<br>Below you will find details and information about the ecodes, which are how you will be compensated for participation in the study.");
                message.Append("<br>Thank you for sharing your voice! ");
                message.Append("If you have any questions, please contact " + _configuration["AppSettings:SpeechAccessibilityTeamEmail"] + ".");
                message.Append("<br><br>Sincerely,");
                message.Append("<br>The Speech Accessibility Project Team");
                message.Append("<br>University of Illinois Urbana - Champaign");

                //added for ecodes information
                message.Append("<br><br>");
                message.Append("<b>Will I be compensated and how does it work?</b>");
                message.Append("<br>");
                message.Append(
                    "Participants receive $60 gift codes, three times during their participation. Those completing the full project will receive a total of $180 in three increments, each occurring approximately every one-third of the way through the recordings. Payments are Amazon eCodes, sent to the email address you provided when you signed up. It may take a few days after completion for your gift card to arrive in your email. We often issue eCodes on Wednesday and Fridays.");
                message.Append("<br>");
                message.Append("The beginning of the email will read:");
                message.Append("<br>");
                message.Append(
                    "Dear Participant, <br>Thank you for participating in the Speech Accessibility Project! We really appreciate your help. Below is the information about your Amazon.com* claim code.");
                message.Append("<br>Amount: $0.00");
                message.Append("<br><b>Claim code: xxxx-xxxxxx-xxxx</b>");
                message.Append("<br><br>");
                message.Append(
                    "<b>I have a caregiver helping me through the project. How will my caregiver be compensated?</b>");
                message.Append("<br>");
                message.Append(
                    "When you sign up for the study, you have the option to enter an email address for your caregiver. That person will be compensated with up to $90 in Amazon eCodes in three increments, each occurring approximately every one-third of the way through the participant recordings.");
                message.Append("<br><br>");
                message.Append("<b>What if I didn’t enter the name of my caregiver at the beginning?</b>");
                message.Append("<br>");
                message.Append(
                    "Let your mentor know and they can assist you. You can also email: "  +_configuration["AppSettings:SpeechAccessibilityTeamEmail"] + " Or, call 1-888-309-6499.");
                message.Append("<br><br>");
                message.Append("<b>I just finished [1/3rd, 2/3rd or all] of my recordings. Where’s my eCode?</b>");
                message.Append("<br>");
                message.Append(
                    "Our payment coordinator sends eCodes twice a week: often on Wednesdays and Fridays. If you finish a block on Saturday, you likely won’t receive your eCode until the middle of the following week.");
                message.Append("<br><br>");
                message.Append("<b>I never got my eCode. Why?</b>");
                message.Append("<br>");
                message.Append(
                    "Sometimes, eCodes go directly to your spam folder. Please check there first. It will come from the Speech Accessibility Project email at "  +_configuration["AppSettings:SpeechAccessibilityTeamEmail"] + ". Our payment coordinator does not send eCodes every day and never on weekends. If you finish a block on Saturday, you will not receive your eCode for a few days");

                message.Append("<br><br>");
                message.Append("<b>Can I be compensated with anything other than Amazon eCodes?</b>");
                message.Append("<br>");
                message.Append("Unfortunately, our project cannot compensate you in any other form at this time.");
                emailSubject = "Your registration has been approved.";

                error = await SendEmailToContributor(contributor, emailSubject, message, _configuration["AppSettings:EmailServer"]);
            }
            else if (action == 3)//deny
            {
                message.Append("Dear " + contributor.FirstName);
                message.Append("<br>Thank you so much for your interest in participating in the Speech Accessibility Project.");
                message.Append("<br>Unfortunately, you do not meet the current criteria for the project. If you have specific questions about this,");
                message.Append("please contact " + _configuration["AppSettings:SpeechAccessibilityTeamEmail"] + ".");
                message.Append("<br><br>Sincerely,");
                message.Append("<br>The Speech Accessibility Project Team");
                message.Append("<br>University of Illinois Urbana - Champaign");

                emailSubject = "Your registration has been denied.";

                error = await SendEmailToContributor(contributor, emailSubject, message, _configuration["AppSettings:EmailServer"]);

            }

            //logging the email
            if (action is 2 or 3)
            {
              
                AddEmailLogging(contributor.Id, emailSubject, contributor.EmailAddress, message, error);
            }

            if (sendTeamForEtiologyChanged)
            {
                //get team email address
                var teamEmailAddress = _etiologyContactEmailAddressRepository.Find(e => e.EtiologyId == etiologyId)
                    .FirstOrDefault();
                if (teamEmailAddress != null)
                {
                    StringBuilder teamMessage = new StringBuilder();
                    var teamEmailSubject = "";
                    var teamError = "";

                    teamMessage.Append("Hello,<br/>Participant, " +
                                       contributor.FirstName + " " + contributor.LastName + ", ");
                    if (action == 1)
                    {
                        teamMessage.Append("was moved from " + oldEtiologyName + " etiology to your team.<br/><br/>");
                        teamEmailSubject = "A participant was moved from " + oldEtiologyName + " etiology to your team";
                    }
                    else
                    {
                        teamMessage.Append("was approved and moved from " + oldEtiologyName + " etiology to your team.< br />< br /> ");
                        teamEmailSubject = "A participant was moved from " + oldEtiologyName + " etiology to your team";
                    }

                 

                    teamMessage.Append("The Speech Accessibility Project Team<br/>University of Illinois Urbana-Champaign");
                   


                    teamError = await SendEmailToTeamForEtiologyChanged(teamEmailAddress.EmailAddress, teamEmailSubject, teamMessage, _configuration["AppSettings:EmailServer"]);
                    if (string.IsNullOrEmpty(teamError))
                    {
                        AddEmailLogging(contributor.Id, teamEmailSubject, teamEmailAddress.EmailAddress, teamMessage, error);
                    }

                }

            }

            return Json(!string.IsNullOrEmpty(error) ? new { Success = false, Message = error } : new { Success = true, Message = "" });
        }

        private void AddEmailLogging(Guid contributorId, string subject, string sendTo, StringBuilder message, string error)
        {
            var emailLogging = new EmailLogging
            {
                ContributorId = contributorId,
                Subject = subject,
                SendTo = sendTo,
                Message = message.ToString(),
                Error = error,
                SendTS = DateTime.Now,
                SendBy = User.Identity.Name
            };
            _emailLoggingRepository.Insert(emailLogging);
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
                    CreateTS = DateTime.Now,
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

            var contributors = _contributorViewRepository.Find(c => c.StatusId == 2)
               .Where(c => c.Id.ToString().Contains(searchValue) || c.FirstName.Contains(searchValue) || c.LastName.Contains(searchValue) || c.EtiologyName.Contains(searchValue) );

            var recordsTotal = contributors.Count();
            contributors = DynamicSortingExtensions<ContributorView>.SetOrderByDynamic(contributors, Request.Form);
            contributors = contributors.Skip(skip).Take(pageSize);
           
            
            return Json(new { draw, data = contributors, recordsFiltered = recordsTotal, recordsTotal = recordsTotal });

        }

        [Authorize(Policy = "AllAnnotatorAndExternalSLPAnnotator")]
        [HttpPost]
        public async Task<ActionResult> EditContributorInfo(Guid contributorId,string contributorEmail, string helperInd, string helperEmail,string birthYear,int subStatusId
            , int subRole, string comments, string helperPhone)
        {
            //contributor email cannot be empty
            if (string.IsNullOrEmpty(contributorEmail))
            {
                return Json(new { Success = false, Message = "Contributor email cannot be empty." });
            }

            //make sure the External annotator has permission for this subrole
            var hasSubRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.OtherPhone)?.Value;
            if (hasSubRole == "Yes")
            {
                if (UtilsExtension.IsMatchedRole(_userSubRoleRepository, subRole, User.Identity.Name) == false)
                    return Json(new { Success = false, Message = "You do not have permission for this contributor." });

            }

            var contributor = _contributorRepository.Find(c => c.Id == contributorId).Include(c=>c.IdentityUser).FirstOrDefault();
            if (contributor == null)
            {
                return Json(new { Success = false, Message = "Contributor is not found." });
            }

            if (contributor.EmailAddress!= null && !contributor.EmailAddress.Equals(contributorEmail))
            {
                contributor.EmailAddress = contributorEmail;
                if (contributor.IdentityUser != null)
                {
                    contributor.IdentityUser.UserName = contributorEmail;
                    contributor.IdentityUser.NormalizedEmail = contributorEmail;
                    contributor.IdentityUser.Email = contributorEmail;
                    contributor.IdentityUser.NormalizedUserName = contributorEmail;
                }
            }
            //else
            //{
            //    contributor.EmailAddress = contributorEmail;
            //    contributor.IdentityUser.UserName = contributorEmail;
            //    contributor.IdentityUser.NormalizedEmail = contributorEmail;
            //    contributor.IdentityUser.Email = contributorEmail;
            //    contributor.IdentityUser.NormalizedUserName = contributorEmail;
            //}

            contributor.HelperInd = helperInd;
            if (helperInd.Trim() == "No")
            {
                contributor.HelperEmail = "";
                contributor.HelperPhoneNumber = "";
            }
            else
            {
                contributor.HelperEmail = helperEmail;
                contributor.HelperPhoneNumber = helperPhone;
            }

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

        [Authorize(Policy = "AllAnnotatorAndExternalSLPAnnotator")]
        public ActionResult ScheduleFollowUpEmail(Guid contributorId)
        {
            var contributor = _contributorRepository.Find(c => c.Id == contributorId).FirstOrDefault();
            if(contributor == null)
                return PartialView("_ScheduleFollowUpEmail", null);

            var scheduledEmail = new ScheduledFollowUpEmailViewModel
            {
                ContributorId = contributorId,
                FirstName = contributor.FirstName,
                LastName = contributor.LastName,
                EmailAddress = contributor.EmailAddress,
                HelperEmail = contributor.HelperEmail
               
            };
            //get existing scheduled email
            var existing = _contributorFollowUpRepository
                .Find(s => s.ContributorId == contributorId && s.EmailSentDate == null).FirstOrDefault();
            if (existing == null)
            {
                scheduledEmail.ScheduledSendDate = DateTime.Now;
                scheduledEmail.EmailContent = "Dear " + contributor.FirstName + Environment.NewLine + Environment.NewLine 
                                              + "Sincerely,"+ Environment.NewLine + "Speech Accessibility Project Team." 
                                              +Environment.NewLine + "University of Illinois Urbana - Champaign";
            }
            else
            {
                scheduledEmail.ScheduledSendDate = existing.ScheduledSendDate;
                scheduledEmail.SendToContributor = existing.SendToContributor;
                scheduledEmail.SendToHelper = existing.SendToHelper;
                scheduledEmail.SendToMentor = existing.SendToMentor;
                scheduledEmail.MentorEmailAddress=existing.MentorEmailAddress;
                scheduledEmail.EmailContent = existing.EmailContent;

            }

            return PartialView("_ScheduleFollowUpEmail", scheduledEmail);
        }

        [Authorize(Policy = "AllAnnotatorAndExternalSLPAnnotator")]
        [HttpPost]
        public async Task<ActionResult> SaveScheduleFollowUpEmail(Guid contributorId, DateTime scheduledSendDate, string sendToContributor, string sendToHelper
            ,string sendToMentor, string mentorEmail, string emailContent)
        {
            var contributor = _contributorRepository.Find(c => c.Id == contributorId).FirstOrDefault();
            if (contributor == null)
            {
                return Json(new { Success = false, Message = "Contributor not found" });
            }

            var returnMessage = "";
            var scheduleFollowUp = _contributorFollowUpRepository.Find(s => s.ContributorId == contributorId && s.EmailSentDate == null).FirstOrDefault();
            if (scheduleFollowUp == null)
            {
                scheduleFollowUp = new ContributorFollowUp();
            }
            scheduleFollowUp.ContributorId = contributorId;
            scheduleFollowUp.ScheduledSendDate = scheduledSendDate;
            scheduleFollowUp.SendToContributor = sendToContributor;
            scheduleFollowUp.SendToHelper = sendToHelper;
            scheduleFollowUp.SendToMentor = sendToMentor;
            scheduleFollowUp.MentorEmailAddress = mentorEmail;
            scheduleFollowUp.EmailContent = emailContent;
            scheduleFollowUp.CreateTS = DateTime.Now;
            scheduleFollowUp.SendBy = User.Identity.Name;

            //if the SendDate is today, send the email and mark sent in the database
            if (scheduledSendDate.CompareTo(DateTime.Now.Date) <= 0)
            {
                returnMessage = await SendFollowUpEmail(contributor, scheduleFollowUp, _configuration["AppSettings:EmailServer"]);
                if (!string.IsNullOrEmpty(returnMessage))
                {
                    return Json(new { Success = false, Message = "Could not send the email." });
                }

                returnMessage = "Follow-up email was sent.";
                scheduleFollowUp.EmailSentDate= DateTime.Now;
            }
            else
            {
                returnMessage = "Scheduled follow-up email was saved.";
            }

            if (scheduleFollowUp.Id == 0)
            {
                _contributorFollowUpRepository.Insert(scheduleFollowUp);
            }
            else
            {
                _contributorFollowUpRepository.Update(scheduleFollowUp);
            }
            return Json(new { Success = true, Message = returnMessage });
        }

        private async Task<string> SendFollowUpEmail(Contributor contributor, ContributorFollowUp contributorFollowUp, string emailServer)
        {
            var emailService = new EmailService();
            var fromEmailAddress = _configuration["AppSettings:SpeechAccessibilityTeamEmail"];
            string toEmailAddress;
            string[] toEmails;
            StringBuilder followUpMessage = new StringBuilder();
           
            var emailSubject = "Speech Accessibility Project - Follow-up ";


            var emailContent = contributorFollowUp.EmailContent.Replace("\n", "<br />");
            //followUpMessage.Append("<br>" + emailContent);

            var actualToEmailAddress = "";
            if (contributorFollowUp.SendToContributor == "Yes")
            {
                actualToEmailAddress += contributor.EmailAddress;
            }

            if (contributorFollowUp.SendToHelper == "Yes")
            {
                if(actualToEmailAddress != "")
                    actualToEmailAddress = actualToEmailAddress + "," + contributor.HelperEmail;
                else
                    actualToEmailAddress += contributor.HelperEmail;
            }

            if (contributorFollowUp.SendToMentor == "Yes")
            {
                if (actualToEmailAddress != "")
                    actualToEmailAddress = actualToEmailAddress + "," + contributorFollowUp.MentorEmailAddress;
                else
                    actualToEmailAddress += contributorFollowUp.MentorEmailAddress;
            }

            if (_configuration["AppSettings:DeveloperMode"] == "Yes")
            {
                toEmailAddress = _configuration["AppSettings:TestingEmail"];
                emailSubject = "Testing: " + emailSubject;
                followUpMessage.Append("This email was sent in a testing mode. The actual email should be sent to '" + actualToEmailAddress + "'.<br>");

                toEmails = new[] { toEmailAddress };
            }
            else if (_configuration["AppSettings:TestingMode"] == "Yes")
            {
                toEmailAddress = User.Identity.Name + "@illinois.edu";
                emailSubject = "Testing: " + emailSubject;
                followUpMessage.Append("This email was sent in a testing mode.The actual email should be sent to '" + actualToEmailAddress + "'.<br>");
                toEmails = new[] { toEmailAddress };
            }
            else
            {
                toEmailAddress = actualToEmailAddress;
                toEmails = new[] { toEmailAddress };
            }
            followUpMessage.Append("<br>" + emailContent);

           var error = await emailService.SendEmail(fromEmailAddress, toEmails, null, null, emailSubject, followUpMessage, emailServer);

           //logging the email
            var emailLogging = new EmailLogging
            {
                ContributorId = contributor.Id,
                Subject = emailSubject,
                SendTo = actualToEmailAddress,
                Message = followUpMessage.ToString(),
                Error = error,
                SendTS = DateTime.Now,
                SendBy = User.Identity.Name
            };
            _emailLoggingRepository.Insert(emailLogging);
            return error;
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
                body.Append("<span style='background-color:yellow;'>This email was sent in a testing mode. The actual email should be sent to " + actualToEmailAddress +
                            ".</span><br>");

                toEmails = new[] { toEmailAddress };
            }
            else if (_configuration["AppSettings:TestingMode"] == "Yes")
            {
                toEmailAddress = User.Identity.Name + "@illinois.edu";
                subject = "Testing: " + emailSubject;
                body.Append("<span style='background-color:yellow;'>This email was sent in a testing mode.The actual email should be sent to " + actualToEmailAddress +
                            ".</span><br>");
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

        private async Task<string> SendEmailToTeamForEtiologyChanged(string toEmail, string emailSubject, StringBuilder emailContent, string emailServer)
        {
            string toEmailAddress;
            string subject;
            StringBuilder body = new StringBuilder();
            var fromEmailAddress = _configuration["AppSettings:SpeechAccessibilityTeamEmail"];
            var emailService = new EmailService();
            var actualToEmailAddress = toEmail;
            string[] toEmails;
            if (_configuration["AppSettings:DeveloperMode"] == "Yes")
            {
                toEmailAddress = _configuration["AppSettings:TestingEmail"];
                subject = "Testing: " + emailSubject;
                body.Append("<span style='background-color:yellow;'>This email was sent in a testing mode. The actual email should be sent to " + actualToEmailAddress +
                            ".</span><br>");

                toEmails = new[] { toEmailAddress };
            }
            else if (_configuration["AppSettings:TestingMode"] == "Yes")
            {
                toEmailAddress = User.Identity.Name + "@illinois.edu";
                subject = "Testing: " + emailSubject;
                body.Append("<span style='background-color:yellow;'>This email was sent in a testing mode.The actual email should be sent to " + actualToEmailAddress +
                            ".</span><br>");
                toEmails = new[] { toEmailAddress };
            }
            else
            {
                toEmailAddress = actualToEmailAddress;
                subject = emailSubject;
                toEmails = new[] { toEmailAddress };
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

            var contributors = _contributorViewRepository.Find(c => c.EtiologyId == subRole && c.StatusId == 4)
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
           return _contributorSubStatusRepository.Find(s => s.StatusId == 2).OrderBy(s => s.DisplayOrder)
               .Select(a => new SelectListItem()
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                })
                .ToList();
        }
    }
}
