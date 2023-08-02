﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SpeechAccessibility.Annotator.Extensions;
using SpeechAccessibility.Annotator.Models;
using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;
using SpeechAccessibility.Infrastructure.Data;

namespace SpeechAccessibility.Annotator.Controllers
{
    [Authorize(Policy = "CompensatorAndAnnotatorAdmin")]
    public class ReportController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IContributorAssignedAnnotatorRepository _contributorAssignedAnnotatorRepository;
        private readonly IContributorRepository _contributorRepository;
        private readonly IRecordingRepository _recordingRepository;
        private readonly IContributorCompensationRepository _contributorCompensationRepository;
        private readonly IConfiguration _configuration;
        private readonly IBlockMasterRepository _blockMasterRepository;
    

        public ReportController(IUserRepository userRepository, IContributorAssignedAnnotatorRepository contributorAssignedAnnotatorRepository, IContributorRepository contributorRepository, IRecordingRepository recordingRepository, IContributorCompensationRepository contributorCompensationRepository, IConfiguration configuration, IBlockMasterRepository blockMasterRepository)
        {
            _userRepository = userRepository;
            _contributorAssignedAnnotatorRepository = contributorAssignedAnnotatorRepository;
            _contributorRepository = contributorRepository;
            _recordingRepository = recordingRepository;
            _contributorCompensationRepository = contributorCompensationRepository;
            _configuration = configuration;
            _blockMasterRepository = blockMasterRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ViewAnnotatorAssignedContributors()
        {
            ViewBag.ExistingAnnotators = _userRepository.Find(u => u.RoleId == 4 & u.Active == "Yes")
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

            return assignedContributors.Any() ? Json(new { Counter = assignedContributors.Count, Contributors = assignedContributors }) : Json(new { Counter = 0 });
        }

        //contributors report
        public IActionResult ViewContributorsRecordingProgress()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetContributorsRecordingProgressforSelectedDates(DateTime? startdate, DateTime? enddate)
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            IQueryable<Contributor> approvedContributors;
            List<ContributorRecordingProgressViewModel> recordings = new List<ContributorRecordingProgressViewModel>();
            if (startdate is null && enddate is null)
            {
                approvedContributors = _contributorRepository.Find(c => c.StatusId == 2)
                    .Where(c => c.Id.ToString().Contains(searchValue.ToLower()) || c.LastName.Contains(searchValue) || c.FirstName.Contains(searchValue) );
            }
            else
            {
                 approvedContributors = _contributorRepository.Find(c => c.StatusId == 2 && c.ApproveTS >= startdate && c.ApproveTS <= enddate)
                     .Where(c => c.Id.ToString().Contains(searchValue.ToLower()) || c.LastName.Contains(searchValue) || c.FirstName.Contains(searchValue));
            }
           
            foreach (var contributor in approvedContributors)
            {
              
                var lastRecording = _recordingRepository.Find(r => r.BlockId != null && r.ContributorId==contributor.Id)
                    .OrderByDescending(r => r.CreateTS).Include(b => b.Block).FirstOrDefault();
                if (lastRecording is not null)
                {
                    ContributorRecordingProgressViewModel temp = new ContributorRecordingProgressViewModel
                    {
                        ContributorId = contributor.Id,
                        LastName = contributor.LastName,
                        FirstName = contributor.FirstName,
                        ApproveTS = (DateTime)contributor.ApproveTS,
                        RecordCreateTS = lastRecording.CreateTS,
                        BlockDescription = "b" + lastRecording.Block.Description + "-" + lastRecording.OriginalPromptId
                    };
                    recordings.Add(temp);
                }
            }
            var recordsTotal = recordings.Count();
            var result = DynamicSortingExtensions<ContributorRecordingProgressViewModel>.SetOrderByDynamic(recordings.AsQueryable(), Request.Form).ToList();
            result = result.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw, recordsFiltered = recordsTotal, recordsTotal, data = result });
          
        }


        public ActionResult ViewDailyContributorSpeechFiles()
        {

            var dailyReport = new DailyContributorSpeechFileReportViewModel
            {
                TodayDate = DateTime.Now
            };
            var yesterdayDate = DateTime.Today.AddDays(-1);
            List<DailyContributorSpeechFileReportViewModel> dailyReportList =
                new List<DailyContributorSpeechFileReportViewModel>();

            var todayDate = DateTime.Now;
            for (var i = 1; i <= 30; i++)
            {
                var newContributorIDs = _contributorRepository.Find(c => DateTime.Compare(c.ApproveTS.Value.Date, todayDate.Date) == 0 && c.StatusId == 2).Select(c => c.Id).ToList();
                var newContributorRecordings = _recordingRepository.Find(r => newContributorIDs.Contains(r.ContributorId) && r.CreateTS.Date == todayDate.Date && r.BlockId > 0);
                var existingContributorIDs = _contributorRepository.Find(c => c.ApproveTS.Value.Date < todayDate.Date && c.StatusId == 2).Select(c => c.Id).ToList();
                var existingContributorRecordings = _recordingRepository.Find(r => existingContributorIDs.Contains(r.ContributorId) && !newContributorIDs.Contains(r.ContributorId) && r.CreateTS.Date == todayDate.Date && r.BlockId != null);

                var report = new DailyContributorSpeechFileReportViewModel
                {
                    TodayDate = todayDate,
                    NewContributors = newContributorIDs.Count,
                    NewContributorRecordings = newContributorRecordings.Count(),
                    ExistingContributors = existingContributorIDs.Count,
                    ExistingContributorRecordings = existingContributorRecordings.Count()
                };

                dailyReportList.Add(report);

                todayDate = todayDate.AddDays(-1);

            }

            return View(dailyReportList);
            //var newContributorIDs = _contributorRepository.Find(c=>c.ApproveTS>=yesterdayDate).Select(c=>c.Id).ToList();
            //var newContributorsRecordings = new List<Tuple<Guid, int>>();

            //foreach (var id in newContributorIDs)
            //{
            //    var recordings = _recordingRepository.Find(r => r.ContributorId == id && r.CreateTS>= yesterdayDate).ToList();
            //    if (recordings.Any())
            //    {
            //        newContributorsRecordings.Add(new Tuple<Guid, int>(id, recordings.Count));
            //    }
            //    else
            //    {
            //        newContributorsRecordings.Add(new Tuple<Guid, int>(id,0));
            //    }
            //}
            ////var existingContributorsNewRecordings = new List<Tuple<Guid, int>>();
            //var newRecordings = _recordingRepository.Find(c =>
            //        !newContributorIDs.Contains(c.ContributorId) && c.CreateTS >= yesterdayDate)
            //    .GroupBy(c => c.ContributorId)
            //    .Select(c =>  new Tuple<Guid, int>(c.Key, c.Count()) ).ToList();


            //dailyReport.NewContributorIDs = newContributorIDs;
            //dailyReport.NewContributorWithNumberRecording = newContributorsRecordings;
            //dailyReport.ExistingContributorsNewRecordings = newRecordings;
            //return View(dailyReport);
        }

        //Compensation report
        public IActionResult ViewContributorsCompensation()
        {

            var contributorCompensation = new ContributorCompensationViewModel
            {

                ContributorsQualifyForFirstCard = new List<Tuple<Contributor, int>>(),
                ContributorsQualifyForSecondCard = new List<Tuple<Contributor, int>>(),
                ContributorsQualifyForThirdCard = new List<Tuple<Contributor, int>>()
            };

            var allPaidContributorsId = _contributorCompensationRepository.GetAll();

            Guid[] paidContributorIDs = allPaidContributorsId.Where(c => c.SendFirstCard != null && c.SendSecondCard != null && c.SendThirdCard != null)
                .Select(c => c.ContributorId).ToArray();

            Guid[] paidContributorFor200IDs = allPaidContributorsId.Where(c => c.SendFirstCard != null).Select(c => c.ContributorId).ToArray();
            Guid[] paidContributorFor400IDs = allPaidContributorsId.Where(c => c.SendSecondCard != null).Select(c => c.ContributorId).ToArray();
            Guid[] paidContributorFor600IDs = allPaidContributorsId.Where(c => c.SendThirdCard != null).Select(c => c.ContributorId).ToArray();

            var contributorList = _recordingRepository.Find(c => !paidContributorIDs.Contains(c.ContributorId) && c.BlockId >= 1).GroupBy(c => c.ContributorId)
                .Select(c => new { ContributorId = c.Key, count = c.Count() }).ToList();

            foreach (var contributor in contributorList)
            {
                if (contributor.count >= 155 && !paidContributorFor200IDs.Contains(contributor.ContributorId))
                {
                    contributorCompensation.ContributorsQualifyForFirstCard
                       .Add(new Tuple<Contributor, int>(_contributorRepository.Find(c => !paidContributorFor200IDs.Contains(c.Id)
                           && c.Id.Equals(contributor.ContributorId)).FirstOrDefault(), contributor.count));
                }
                if (contributor.count >= 310 && !paidContributorFor400IDs.Contains(contributor.ContributorId))
                {
                    contributorCompensation.ContributorsQualifyForSecondCard
                        .Add(new Tuple<Contributor, int>(_contributorRepository.Find(c => !paidContributorFor400IDs.Contains(c.Id)
                            && c.Id.Equals(contributor.ContributorId)).FirstOrDefault(), contributor.count));
                }
                if (contributor.count >= 450 && !paidContributorFor600IDs.Contains(contributor.ContributorId))
                {
                    contributorCompensation.ContributorsQualifyForThirdCard
                        .Add(new Tuple<Contributor, int>(_contributorRepository.Find(c => !paidContributorFor600IDs.Contains(c.Id)
                            && c.Id.Equals(contributor.ContributorId)).FirstOrDefault(), contributor.count));
                }
            }
            return View(contributorCompensation);
        }

        [HttpPost]
        public JsonResult GenerateGiftCard(Guid[] contributorIDs, int cardType)
        {
            List<ContributorCompensation> newCompensations = new List<ContributorCompensation>();
            List<GiftCardViewModel> giftCards = new List<GiftCardViewModel>();
            //generate gift card
            var fileName = cardType switch
            {
                1 => "FirstGiftCard" + DateTime.Now.ToString("yyyyMMddHHmm") + ".csv",
                2 => "SecondGiftCard" + DateTime.Now.ToString("yyyyMMddHHmm") + ".csv",
                _ => "ThirdGiftCard" + DateTime.Now.ToString("yyyyMMddHHmm") + ".csv"
            };

            //mark the first gift card paid
            foreach (var id in contributorIDs)
            {
                //check if this contributor is already paid for the first gift card
                var paid = _contributorCompensationRepository.Find(c => c.ContributorId == id).FirstOrDefault();
                if (paid != null)
                {
                    switch (cardType)
                    {
                        case 1:
                            paid.SendFirstCard = DateTime.Now;
                            paid.SendFirstCardBy = User.Identity.Name;
                            break;
                        case 2:
                            paid.SendSecondCard = DateTime.Now;
                            paid.SendSecondCardBy = User.Identity.Name;
                            break;
                        default:
                            paid.SendThirdCard = DateTime.Now;
                            paid.SendThirdCardBy = User.Identity.Name;
                            break;
                    }
                    _contributorCompensationRepository.Update(paid);
                }
                else
                {
                    var contributorComp = new ContributorCompensation
                    {
                        ContributorId = id
                    };
                    switch (cardType)
                    {
                        case 1:
                            contributorComp.SendFirstCard = DateTime.Now;
                            contributorComp.SendFirstCardBy = User.Identity.Name;
                            break;
                        case 2:
                            contributorComp.SendSecondCard = DateTime.Now;
                            contributorComp.SendSecondCardBy = User.Identity.Name;
                            break;
                        default:
                            contributorComp.SendThirdCard = DateTime.Now;
                            contributorComp.SendThirdCardBy = User.Identity.Name;
                            break;
                    }
                    newCompensations.Add(contributorComp);
                }
                //add to gift card list
                var contributor = _contributorRepository.Find(c => c.Id == id).FirstOrDefault();
                if (contributor != null)
                {
                    giftCards.Add(new GiftCardViewModel { Amount = 60.00, Delay = 0, EmailAddress = contributor.EmailAddress });
                    if (contributor.HelperInd == "Yes")
                        giftCards.Add(new GiftCardViewModel { Amount = 30.00, Delay = 0, EmailAddress = contributor.HelperEmail });
                }
            }

            if (newCompensations.Count > 0)
            {
                _contributorCompensationRepository.AddRange(newCompensations);
            }

            if (giftCards.Count > 0)
            {
                ////save the file to server folder
                var basePath = _configuration["AppSettings:UploadFileFolder"] + "\\GiftCards";
                var fullPath = Path.Combine(basePath, fileName);

                using var writer = new StreamWriter(fullPath);
                using var csv = new CsvWriter(writer,
                    new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false });
                csv.WriteRecords(giftCards);
                return Json(new { Success = true, Message = fileName });
            }

            return Json(new { Success = false, Message = "Please select a contributor to generate gift card." });
        }

        public IActionResult ViewContributorsCompensationHistory()
        {
            return View();
        }

        public ActionResult ContributorsCompensationHistory()
        {

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            List<ContributorCompensationHistoryViewModel> compensationHistory = new List<ContributorCompensationHistoryViewModel>();
            var compensationList = _contributorCompensationRepository.GetAll();
            foreach (var compensation in compensationList)
            {
                compensationHistory.Add(new ContributorCompensationHistoryViewModel()
                { Contributor = _contributorRepository.Find(c => c.Id == compensation.ContributorId).FirstOrDefault(), ContributorCompensation = compensation });

            }
            var total = compensationHistory.Count;
            var tempList = DynamicSortingExtensions<ContributorCompensationHistoryViewModel>.SetOrderByDynamic(compensationHistory.AsQueryable(), Request.Form);

            compensationHistory = tempList.Where(comp => comp.Contributor.FirstName.ToLower().Contains(searchValue)
                                                       || comp.Contributor.LastName.ToLower().Contains(searchValue)
                                                       || comp.ContributorCompensation.ContributorId.ToString().Contains(searchValue)).Skip(skip).Take(pageSize)
                .Select(comp => new ContributorCompensationHistoryViewModel()
                {
                    ContributorCompensation = comp.ContributorCompensation,
                    Contributor = comp.Contributor
                }).ToList();
            

            return Json(new { draw = draw, recordsFiltered = total, recordsTotal = total, data = compensationHistory });

        }

        public IActionResult ViewAnnotationProgress()
        {

            DateTime endDate = DateTime.Today;
            DateTime startDate = endDate.AddDays(-7);
            ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.ToString("yyyy-MM-dd");
            return View();
        }
        //public IActionResult ViewAnnotationProgress(DateTime? startDateIn, DateTime? endDateIn)
        //{
        //    List<AnnotationProgressViewModel> list = new List<AnnotationProgressViewModel>();
        //    //set default search dates 
        //    DateTime startDate;
        //    DateTime endDate;
        //    if (endDateIn is null)
        //    {
        //        endDate = DateTime.Today;
        //        if (startDateIn is null)
        //        {
        //            startDate = endDate.AddDays(-7);
        //        }
        //        else
        //        {
        //            startDate = Convert.ToDateTime(startDateIn);
        //        }
        //    }
        //    else
        //    {
        //        endDate = Convert.ToDateTime(endDateIn);

        //        if (startDateIn is null)
        //        {
        //            startDate = endDate.AddDays(-1);
        //        }
        //        else
        //        {
        //            startDate = Convert.ToDateTime(startDateIn);
        //        }
        //    }

        //    ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
        //    ViewBag.EndDate = endDate.ToString("yyyy-MM-dd");

        //    var annotators = _contributorAssignedAnnotatorRepository.Find(a => a.User.Active == "Yes")
        //        .Include(a=>a.User).ToList()
        //            .GroupBy(a => new { a.User });

        //    while (startDate <= endDate)
        //    {
        //        //get each annotator assigned recordings group by status 
        //        foreach (var annotator in annotators)
        //        {
        //            var temp = new AnnotationProgressViewModel
        //            {
        //                ReportDate = startDate,
        //                Annotator = annotator.Key.User
        //            };

        //            //get assigned Contributors
        //            var contributorIds = annotator.ToList().Select(a => a.ContributorId);

        //            temp.AssignedRecordingByContributor = _recordingRepository.Find(r => contributorIds.Contains(r.ContributorId) &&
        //                    r.BlockId != null
        //                    && DateTime.Compare(r.UpdateTS.Date, startDate.Date) ==
        //                    0)
        //                .Include(r => r.Status)
        //                .ToList()
        //                .GroupBy(r => new { r.ContributorId, r.StatusId, r.Status.Name })
        //                .Select(group => new RecordingStatusViewModel()
        //                {
        //                    ContributorId = group.Key.ContributorId,
        //                    Status = group.Key.Name,  
        //                    NumberOfRecord= group.ToList().Count

        //                })
        //                .ToList();

        //            list.Add(temp);
        //        }
        //        startDate = startDate.AddDays(1);
        //    }

        //    return View(list);
        //}

        [HttpPost]
        public ActionResult LoadAnnotatorsForProgress()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var annotators = _contributorAssignedAnnotatorRepository.Find(a => a.User.Active == "Yes" && a.User.LastName.Contains(searchValue) || a.User.FirstName.Contains(searchValue))
                .Include(a => a.User).Distinct();
            var recordsTotal = annotators.Count();
           
            var annotatorList = annotators.Skip(skip).Take(pageSize).Select(p => new User()
            {
                Id = p.User.Id,
                FirstName = p.User.FirstName,
                LastName = p.User.LastName,
                NetId = p.User.NetId
            }).Distinct();
            annotatorList = DynamicSortingExtensions<User>.SetOrderByDynamic(annotatorList, Request.Form);
            return Json(new { draw, data = annotatorList, recordsFiltered = recordsTotal, recordsTotal = recordsTotal });

        }

        [HttpPost]
        public PartialViewResult LoadAnnotatorsWorkingSpeechFiles(int annotatorId, DateTime? startDateIn, DateTime? endDateIn)
        {
            //set default search dates 
            DateTime startDate;
            DateTime endDate;
            if (endDateIn is null)
            {
                endDate = DateTime.Today;
                if (startDateIn is null)
                {
                    startDate = endDate.AddDays(-7);
                }
                else
                {
                    startDate = Convert.ToDateTime(startDateIn);
                }
            }
            else
            {
                endDate = Convert.ToDateTime(endDateIn);
                if (startDateIn is null)
                {
                    startDate = endDate.AddDays(-1);
                }
                else
                {
                    startDate = Convert.ToDateTime(startDateIn);
                    if (DateTime.Compare(endDate.Date,startDate.Date) < 0)
                    {
                        startDate = endDate.AddDays(-1);
                    }
                }
            }

            List<AnnotationProgressViewModel> list = new List<AnnotationProgressViewModel>();
          
            while (startDate <= endDate)
            {
                //get each annotator assigned recordings group by status
                var temp = new AnnotationProgressViewModel
                {
                    ReportDate = startDate,
                    AnnotatorId = annotatorId
                };

                //get assigned Contributors
                var contributorIds = _contributorAssignedAnnotatorRepository.Find(a => a.UserId==annotatorId)
                    .ToList().Select(a => a.ContributorId);

                temp.AssignedRecordingByContributor = _recordingRepository.Find(r => contributorIds.Contains(r.ContributorId) &&
                        r.BlockId != null
                        && DateTime.Compare(r.UpdateTS.Date, startDate.Date) ==
                        0)
                    .Include(r => r.Status)
                    .ToList()
                    .GroupBy(r => new { r.ContributorId, r.StatusId, r.Status.Name })
                    .Select(group => new RecordingStatusViewModel()
                    {
                        ContributorId = group.Key.ContributorId,
                        Status = group.Key.Name,
                        NumberOfRecord = group.ToList().Count,
                        ReportDate = startDate

                    })
                    .ToList();
                list.Add(temp);
                startDate = startDate.AddDays(1);
            }

            return PartialView("_AnnotatorsWorkingSpeechFiles", list);

        }


    }
}
