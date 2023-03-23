
using System;
using System.Linq;
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

        public ReportController(IUserRepository userRepository, IContributorAssignedAnnotatorRepository contributorAssignedAnnotatorRepository, IContributorRepository contributorRepository)
        {
            _userRepository = userRepository;
            _contributorAssignedAnnotatorRepository = contributorAssignedAnnotatorRepository;
            _contributorRepository = contributorRepository;
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
    }
}
