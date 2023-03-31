
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SpeechAccessibility.Core.Interfaces;

namespace SpeechAccessibility.Annotator.Controllers
{
    [Authorize(Policy = "AnnotatorAdmin")]
    public class ReportController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IContributorAssignedAnnotatorRepository _contributorAssignedAnnotatorRepository;
        private readonly IContributorRepository _contributorRepository;
        private readonly IRecordingRepository _recordingRepository;

        public ReportController(IUserRepository userRepository, IContributorAssignedAnnotatorRepository contributorAssignedAnnotatorRepository, IContributorRepository contributorRepository, IRecordingRepository recordingRepository)
        {
            _userRepository = userRepository;
            _contributorAssignedAnnotatorRepository = contributorAssignedAnnotatorRepository;
            _contributorRepository = contributorRepository;
            _recordingRepository = recordingRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ViewAnnotatorAssignedContributors()
        {
            ViewBag.ExistingAnnotators =  _userRepository.Find(u => u.RoleId == 4 & u.Active == "Yes")
                .OrderBy(r => r.LastName).Select(u =>
                    new SelectListItem()
                    {
                        Value = u.Id.ToString(),
                        Text = u.LastName + ", " + u.FirstName
                    })
                .ToList();

            return View();
        }

        

        public ActionResult GetAnnotatorAssignedContributors(int annotatorId)
        {
            
            Guid[] assignedContributorIdList = _contributorAssignedAnnotatorRepository.Find(c => c.UserId == annotatorId)
                .Select(u => u.ContributorId).ToArray();
            //var assignedContributors = _contributorRepository.Find(c => assignedContributorIdList.Contains(c.Id));

            var assignedContributors =
                _contributorRepository.Find(c => assignedContributorIdList.Contains(c.Id)).ToList();

            return assignedContributors.Any() ? Json(new { Counter = assignedContributors.Count, Contributors = assignedContributors }) : Json(new { Counter = 0});
        }

        //contributors report
        public IActionResult ViewContributorsRecordingProgress()
        {
            return View();
        }

        public ActionResult GetContributorsRecordingProgressforSelectedDates(DateTime startdate, DateTime enddate)
        {
            Guid[] contributorProgressList = _recordingRepository.Find(c => c.CreateTS >= startdate && c.CreateTS <= enddate).Select(l => l.ContributorId).ToArray();

            var contributorsProgress = _recordingRepository.Find(c => contributorProgressList.Contains(c.ContributorId)).ToList();
            var result = contributorsProgress
                          .GroupBy(cp => cp.ContributorId)
                          .Select(g => g.OrderByDescending(cp => cp.CreateTS).FirstOrDefault())
                          .OrderBy(cp => cp.ContributorId)
                          .Select(cp => new {
                              cp.ContributorId,
                              cp.OriginalPromptId,
                              cp.CreateTS,
                              cp.BlockId
                          })
                          .ToList();


            return contributorsProgress.Any() ? Json(new { Counter = contributorsProgress.Count, ContributorsProgresslist = result}) : Json(new { Counter = 0});
        }
    }
}
