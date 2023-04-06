
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SpeechAccessibility.Annotator.Models;
using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Infrastructure.Data;

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

        public ActionResult ViewDailyContributorSpeechFiles()
        {

            var dailyReport = new DailyContributorSpeechFileReportViewModel();
            dailyReport.TodayDate = DateTime.Now;
            var yesterdayDate = DateTime.Today.AddDays(-1);
            var newContributorIDs = _contributorRepository.Find(c=>c.ApproveTS>=yesterdayDate).Select(c=>c.Id).ToList();
            var newContributorsRecordings = new List<Tuple<Guid, int>>();
           
            foreach (var id in newContributorIDs)
            {
                var recordings = _recordingRepository.Find(r => r.ContributorId == id && r.CreateTS>= yesterdayDate).ToList();
                if (recordings.Any())
                {
                    newContributorsRecordings.Add(new Tuple<Guid, int>(id, recordings.Count));
                }
                else
                {
                    newContributorsRecordings.Add(new Tuple<Guid, int>(id,0));
                }
            }

            var existingContributorsNewRecordings = new List<Tuple<Guid, int>>();


            var newRecordings = _recordingRepository.Find(c =>
                    !newContributorIDs.Contains(c.ContributorId) && c.CreateTS >= yesterdayDate)
                .GroupBy(c => c.ContributorId)
                .Select(c =>  new Tuple<Guid, int>(c.Key, c.Count()) ).ToList();


            dailyReport.NewContributorIDs = newContributorIDs;
            dailyReport.NewContributorWithNumberRecording = newContributorsRecordings;
            dailyReport.ExistingContributorsNewRecordings = newRecordings;
            return View(dailyReport);
        }

    }
}
