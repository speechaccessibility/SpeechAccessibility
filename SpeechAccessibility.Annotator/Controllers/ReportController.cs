
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

namespace SpeechAccessibility.Annotator.Controllers
{
    [Authorize(Policy = "CompensatorAndAnnotatorAdmin")]
    public class ReportController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IContributorAssignedAnnotatorRepository _contributorAssignedAnnotatorRepository;
        private readonly IContributorViewRepository _contributorViewRepository;
        private readonly IRecordingRepository _recordingRepository;
        private readonly IContributorCompensationRepository _contributorCompensationRepository;
        private readonly IContributorCompensationViewRepository _contributorCompensationViewRepository;
        private readonly IContributorCompensationHistoryRepository _contributorCompensationHistoryRepository;
        private readonly IEtiologyViewRepository _etiologyViewRepository;
        private readonly IConfiguration _configuration;
        private readonly IContributorAssignedListRepository _contributorAssignedListRepository;
        private readonly INumberOfRecordingByEtiologyRepository _numberOfRecordingByEtiologyRepository;
        private readonly IContributorsPaidByCheckRepository _contributorsPaidByCheckRepository;
        private readonly IHelperNotPaidGiftCardsRepository _helperNotPaidGiftCardsRepository;
        private readonly IContributorStatusRepository _contributorStatusRepository;
        private readonly IContributorSubStatusRepository _contributorSubStatusRepository;

        public ReportController(IUserRepository userRepository, IContributorAssignedAnnotatorRepository contributorAssignedAnnotatorRepository
            , IContributorViewRepository contributorViewRepository, IRecordingRepository recordingRepository, IContributorCompensationRepository contributorCompensationRepository
            , IConfiguration configuration, IContributorCompensationViewRepository contributorCompensationViewRepository, IContributorCompensationHistoryRepository contributorCompensationHistoryRepository, IEtiologyViewRepository etiologyViewRepository, IContributorAssignedListRepository contributorAssignedListRepository, INumberOfRecordingByEtiologyRepository numberOfRecordingByEtiologyRepository, IContributorsPaidByCheckRepository contributorsPaidByCheckRepository, IHelperNotPaidGiftCardsRepository helperNotPaidGiftCardsRepository, IContributorStatusRepository contributorStatusRepository, IContributorSubStatusRepository contributorSubStatusRepository)
        {
            _userRepository = userRepository;
            _contributorAssignedAnnotatorRepository = contributorAssignedAnnotatorRepository;
            _contributorViewRepository = contributorViewRepository;
            _recordingRepository = recordingRepository;
            _contributorCompensationRepository = contributorCompensationRepository;
            _configuration = configuration;
            _contributorCompensationViewRepository = contributorCompensationViewRepository;
            _contributorCompensationHistoryRepository = contributorCompensationHistoryRepository;
            _etiologyViewRepository = etiologyViewRepository;
            _contributorAssignedListRepository = contributorAssignedListRepository;
            _numberOfRecordingByEtiologyRepository = numberOfRecordingByEtiologyRepository;
            _contributorsPaidByCheckRepository = contributorsPaidByCheckRepository;
            _helperNotPaidGiftCardsRepository = helperNotPaidGiftCardsRepository;
            _contributorStatusRepository = contributorStatusRepository;
            _contributorSubStatusRepository = contributorSubStatusRepository;
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
           

            var assignedContributors =
                _contributorViewRepository.Find(c => assignedContributorIdList.Contains(c.Id)).OrderBy(c => c.EtiologyName).ToList();

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
            IQueryable<ContributorView> approvedContributors;
            List<ContributorRecordingProgressViewModel> recordings = new List<ContributorRecordingProgressViewModel>();
            if (startdate is null && enddate is null)
            {
                approvedContributors = _contributorViewRepository.Find(c => c.StatusId == 2)
                    .Where(c => c.Id.ToString().Contains(searchValue.ToLower()) || c.LastName.Contains(searchValue) || c.FirstName.Contains(searchValue) );
            }
            else
            {
                 approvedContributors = _contributorViewRepository.Find(c => c.StatusId == 2 && c.ApproveTS >= startdate && c.ApproveTS <= enddate)
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
           
            var yesterdayDate = DateTime.Today.AddDays(-1);
            List<DailyContributorSpeechFileReportViewModel> dailyReportList =
                new List<DailyContributorSpeechFileReportViewModel>();
            var etiologies = CacheExtension.GetEtiologyList(_etiologyViewRepository);

            var todayDate = DateTime.Now;
            //var todayDate = Convert.ToDateTime("11/30/2023");
            for (var i = 1; i <= 30; i++)
            {
                var dailyReport = new DailyContributorSpeechFileReportViewModel();
                dailyReport.TodayDate = todayDate;
                dailyReport.ApprovedContributors = new List<int>();
                dailyReport.ApprovedContributorRecordings = new List<int>();
                dailyReport.NewContributors = new List<int>();


                foreach (var etiology in etiologies)
                {
                    var newContributors =
                        _contributorViewRepository.Find(c => c.CreateTS.Date == todayDate.Date && (c.StatusId==1 || c.StatusId==5 )&& c.EtiologyId == etiology.Id);

                    var approvedContributors =
                        _contributorViewRepository.Find(c => c.ApproveTS.Value.Date <= todayDate.Date && c.StatusId == 2 && c.EtiologyId==etiology.Id);

                    var approvedContributorIDs = approvedContributors.Select(c => c.Id).ToList();
                    var approvedContributorRecordings = _recordingRepository.Find(r => approvedContributorIDs.Contains(r.ContributorId)
                        && r.CreateTS.Date == todayDate.Date && r.BlockId != null);

                    dailyReport.NewContributors.Add(newContributors?.Count() ?? 0);
                    dailyReport.ApprovedContributors.Add(approvedContributors?.Count() ?? 0);
                    dailyReport.ApprovedContributorRecordings.Add(approvedContributorRecordings?.Count() ?? 0);
                }

                dailyReportList.Add(dailyReport);
                todayDate = todayDate.AddDays(-1);

            }

            return View(dailyReportList);
           
        }

        //Compensation report
        public IActionResult ViewContributorsCompensation()
        {
            var contributorsPayByCheck = _contributorsPaidByCheckRepository.GetAll().Select(c => c.EmailDomain).ToList();
            var helperNotGetPay = _helperNotPaidGiftCardsRepository.GetAll().Select(h => h.HelperEmailAddress).ToList();
            

            var contributorCompensation = new ContributorCompensationViewModel
            {

                ContributorsQualifyForFirstCard = new List<ContributorCompensationView>(),
                ContributorsQualifyForSecondCard = new List<ContributorCompensationView>(),
                ContributorsQualifyForThirdCard = new List<ContributorCompensationView>()
            };

            var allGiftCards = _contributorCompensationViewRepository
                .Find(g=>g.PaymentType=="ecode" && (g.FirstCard=="Yes" || g.SecondCard=="Yes" || g.ThirdCard=="Yes"));

            var firstCardList =  allGiftCards.Where(g => g.FirstCard == "Yes").OrderBy(g => g.LastName).ThenBy(g => g.FirstName).ToList();
            int i;
            for (i = firstCardList.Count - 1; i >= 0; i--)
            {
                var card = firstCardList[i];
               
                foreach (var contributor in contributorsPayByCheck)
                {
                    //remove contributors paid by check
                    if (card.EmailAddress.Contains(contributor))
                    {
                        firstCardList.Remove(card);
                        break;
                    }
                    //remove helpers should not get paid
                    foreach (var helper in helperNotGetPay)
                    {
                        if (!string.IsNullOrEmpty(card.HelperEmail) && card.HelperEmail.Contains(helper))
                            card.HelperInd = "No";
                    }
                }
                
            }
            contributorCompensation.ContributorsQualifyForFirstCard = firstCardList;

            var secondCardList = allGiftCards.Where(g => g.SecondCard == "Yes")
                .OrderBy(g => g.LastName).ThenBy(g => g.FirstName).ToList();
            for (i = secondCardList.Count - 1; i >= 0; i--)
            {
                var card = secondCardList[i];
                foreach (var contributor in contributorsPayByCheck)
                {
                    //remove contributors paid by check
                    if (card.EmailAddress.Contains(contributor))
                    {
                        secondCardList.Remove(card);
                        break;
                    }
                    //remove helpers should not get paid
                    foreach (var helper in helperNotGetPay)
                    {
                        if (!string.IsNullOrEmpty(card.HelperEmail) && card.HelperEmail.Contains(helper))
                            card.HelperInd = "No";
                    }
                }
               
            }
            contributorCompensation.ContributorsQualifyForSecondCard = secondCardList;

            var thirdCardList = allGiftCards.Where(g => g.ThirdCard == "Yes")
                .OrderBy(g => g.LastName).ThenBy(g => g.FirstName).ToList();
            for (i = thirdCardList.Count - 1; i >= 0; i--)
            {
                var card = thirdCardList[i];
                foreach (var contributor in contributorsPayByCheck)
                {
                    //remove helpers should not get paid
                    if (card.EmailAddress.Contains(contributor))
                    {
                        thirdCardList.Remove(card);
                        break;
                    }
                    //remove helpers should not get paid
                    foreach (var helper in helperNotGetPay)
                    {
                        if (!string.IsNullOrEmpty(card.HelperEmail) && card.HelperEmail.Contains(helper))
                            card.HelperInd = "No";
                    }
                }
               
               
            }
            contributorCompensation.ContributorsQualifyForThirdCard = thirdCardList;


           
            return View(contributorCompensation);
        }

        public IActionResult ViewContributorsCompensationByCheck()
        {
            //List<string> contributorsPayByCheck = new List<string>() { "@aol.com", "@att", "@icloud.com" };
            //List<string> helperNotGetPay = new List<string>() { "facamer2@gmail.com", "charr68@gmail.com" };
            var contributorsPayByCheck = _contributorsPaidByCheckRepository.GetAll().Select(c => c.EmailDomain).ToList();
            var helperNotGetPay = _helperNotPaidGiftCardsRepository.GetAll().Select(h => h.HelperEmailAddress).ToList();
            var contributorCompensation = new ContributorCompensationViewModel
            {
                ContributorsQualifyForFirstCard = new List<ContributorCompensationView>(),
                ContributorsQualifyForSecondCard = new List<ContributorCompensationView>(),
                ContributorsQualifyForThirdCard = new List<ContributorCompensationView>()
            };

            var allGiftCards = _contributorCompensationViewRepository
                .Find(g => g.FirstCard == "Yes" || g.SecondCard == "Yes" || g.ThirdCard == "Yes");

            var firstCardList = allGiftCards.Where(g => g.FirstCard == "Yes").OrderBy(g => g.LastName)
                .ThenBy(g => g.FirstName);
          
            var contributorsPaidByCheckFirstCard = new List<ContributorCompensationView>();

            foreach (var card in firstCardList)
            {
                //if contributor is paid by check, add to the list
                if (card.PaymentType == "check")
                {
                    contributorsPaidByCheckFirstCard.Add(card);
                    //remove helpers should not get paid
                    foreach (var helper in helperNotGetPay)
                    {
                        if (!string.IsNullOrEmpty(card.HelperEmail) && card.HelperEmail.Contains(helper))
                        {
                            card.HelperInd = "No";
                        }
                    }
                }
                else
                {
                    //only keep contributors paid by check 
                    foreach (var contributor in contributorsPayByCheck)
                    {
                        //only keep contributors paid by check 
                        if (card.EmailAddress.Contains(contributor))
                        {
                            //remove helpers should not get paid
                            foreach (var helper in helperNotGetPay)
                            {
                                if (!string.IsNullOrEmpty(card.HelperEmail) && card.HelperEmail.Contains(helper))
                                {
                                    card.HelperInd = "No";
                                }
                            }

                            contributorsPaidByCheckFirstCard.Add(card);
                            break;
                        }
                    }
                }
            }

            
            contributorCompensation.ContributorsQualifyForFirstCard = contributorsPaidByCheckFirstCard;

            var secondCardList = allGiftCards.Where(g => g.SecondCard == "Yes")
                .OrderBy(g => g.LastName).ThenBy(g => g.FirstName).ToList();

            var contributorsPaidByCheckSecondCard = new List<ContributorCompensationView>();

            foreach (var card in secondCardList)
            {
                //if contributor is paid by check, add to the list
                if (card.PaymentType == "check")
                {
                    contributorsPaidByCheckSecondCard.Add(card);
                    //remove helpers should not get paid
                    foreach (var helper in helperNotGetPay)
                    {
                        if (!string.IsNullOrEmpty(card.HelperEmail) && card.HelperEmail.Contains(helper))
                        {
                            card.HelperInd = "No";
                        }
                    }
                }
                else
                {
                    //only keep contributors paid by check 
                    foreach (var contributor in contributorsPayByCheck)
                    {
                        //only keep contributors paid by check 
                        if (card.EmailAddress.Contains(contributor))
                        {
                            //remove helpers should not get paid
                            foreach (var helper in helperNotGetPay)
                            {
                                if (!string.IsNullOrEmpty(card.HelperEmail) && card.HelperEmail.Contains(helper))
                                {
                                    card.HelperInd = "No";
                                }
                            }

                            contributorsPaidByCheckSecondCard.Add(card);
                            break;
                        }
                    }
                }
            }
            contributorCompensation.ContributorsQualifyForSecondCard = contributorsPaidByCheckSecondCard;

            var thirdCardList = allGiftCards.Where(g => g.ThirdCard == "Yes")
                .OrderBy(g => g.LastName).ThenBy(g => g.FirstName).ToList();
            var contributorsPaidByCheckThirdCard = new List<ContributorCompensationView>();

            foreach (var card in thirdCardList)
            {
                //if contributor is paid by check, add to the list
                if (card.PaymentType == "check")
                {
                    contributorsPaidByCheckThirdCard.Add(card);
                    //remove helpers should not get paid
                    foreach (var helper in helperNotGetPay)
                    {
                        if (!string.IsNullOrEmpty(card.HelperEmail) && card.HelperEmail.Contains(helper))
                        {
                            card.HelperInd = "No";
                        }
                    }

                }
                else
                {
                    //only keep contributors paid by check 
                    foreach (var contributor in contributorsPayByCheck)
                    {
                        //only keep contributors paid by check 
                        if (card.EmailAddress.Contains(contributor))
                        {
                            //remove helpers should not get paid
                            foreach (var helper in helperNotGetPay)
                            {
                                if (!string.IsNullOrEmpty(card.HelperEmail) && card.HelperEmail.Contains(helper))
                                {
                                    card.HelperInd = "No";
                                }
                            }

                            //need to add first recording date and last recording date for payment by check
                            var recordingList = _recordingRepository.Find(r => r.ContributorId == card.ContributorId);

                            var firstRecordingDate = recordingList.Min(r => r.CreateTS);
                            var lastRecordingDate = recordingList.Max(r => r.CreateTS);
                            card.FirstRecordingDate = firstRecordingDate;
                            card.LastRecordingDate = lastRecordingDate;

                            contributorsPaidByCheckThirdCard.Add(card);
                            break;
                        }
                    }
                }
            }
            contributorCompensation.ContributorsQualifyForThirdCard = contributorsPaidByCheckThirdCard;


            return View(contributorCompensation);
        }


        [HttpPost]
        public JsonResult GenerateGiftCard(Guid[] contributorIDs, int cardType, string paymentType)
        {
            List<ContributorCompensation> newCompensations = new List<ContributorCompensation>();
            List<GiftCardViewModel> giftCards = new List<GiftCardViewModel>();
            List< GiftCardByCheckViewModel> giftCardsByCheck = new List<GiftCardByCheckViewModel>();

            //generate gift card
            var fileName = "";
            if (paymentType == "check") //if payment by check, we issue only one check for all gift cards, the cardType should always=3
            {
                //fileName = cardType switch
                //{
                //    1 => "FirstGiftCardByCheck" + DateTime.Now.ToString("yyyyMMddHHmm") + ".csv",
                //    2 => "SecondGiftCardByCheck" + DateTime.Now.ToString("yyyyMMddHHmm") + ".csv",
                //    _ => "ThirdGiftCardByCheck" + DateTime.Now.ToString("yyyyMMddHHmm") + ".csv"
                //};
                fileName = "AllGiftCardsCheck" + DateTime.Now.ToString("yyyyMMddHHmm") + ".csv";
            }
            else
            {
                fileName = cardType switch
                {
                    1 => "FirstGiftCard" + DateTime.Now.ToString("yyyyMMddHHmm") + ".csv",
                    2 => "SecondGiftCard" + DateTime.Now.ToString("yyyyMMddHHmm") + ".csv",
                    _ => "ThirdGiftCard" + DateTime.Now.ToString("yyyyMMddHHmm") + ".csv"
                };
            }
            
            
            foreach (var id in contributorIDs)
            {
                var contributor = _contributorViewRepository.Find(c => c.Id == id).FirstOrDefault();
                bool includeHelper = true;
                if (contributor != null)
                {
                    if (contributor.HelperInd.Trim() == "Yes")
                    {
                        //check to make that this helper is not in the "should not get payment" list
                        var helpersShouldNotGetPaid = _helperNotPaidGiftCardsRepository.GetAll();

                        foreach (var helper in helpersShouldNotGetPaid)
                        {
                            if (helper.HelperEmailAddress.Equals(contributor.HelperEmail))
                            {
                                includeHelper = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        includeHelper = false;
                    }
                }

                //check if this contributor is already paid for the first gift card
                //if paid by check, should return null if not paid yet, otherwise this contributor should not be on the list
                var paid = _contributorCompensationRepository.Find(c => c.ContributorId == id).FirstOrDefault();
                if (paid != null)
                {
                    switch (cardType)
                    {
                        case 1:
                            paid.PaidHelper = includeHelper ? contributor.HelperEmail : "";
                            paid.PaymentType = paymentType;
                            paid.SendFirstCard = DateTime.Now;
                            paid.SendFirstCardBy = User.Identity.Name;
                            break;
                        case 2:
                            paid.PaidHelper = includeHelper ? contributor.HelperEmail : "";
                            paid.PaymentType = paymentType;
                            paid.SendSecondCard = DateTime.Now;
                            paid.SendSecondCardBy = User.Identity.Name;
                            break;
                        default:
                            paid.PaidHelper = includeHelper ? contributor.HelperEmail : "";
                            paid.PaymentType = paymentType;
                            paid.SendThirdCard = DateTime.Now;
                            paid.SendThirdCardBy = User.Identity.Name;
                            break;
                    }

                    _contributorCompensationRepository.Update(paid);
                }
                else //if paid by check, should mark all gift cards paid
                {
                    var contributorComp = new ContributorCompensation
                    {
                        ContributorId = id
                    };
                    if (paymentType == "check")
                    {
                        contributorComp.PaidHelper = includeHelper ? contributor.HelperEmail : "";
                        contributorComp.SendFirstCard = DateTime.Now;
                        contributorComp.SendFirstCardBy = User.Identity.Name;
                       
                        contributorComp.PaidHelper = includeHelper ? contributor.HelperEmail : "";
                        contributorComp.SendSecondCard = DateTime.Now;
                        contributorComp.SendSecondCardBy = User.Identity.Name;
                        
                        contributorComp.PaidHelper = includeHelper ? contributor.HelperEmail : "";
                        contributorComp.SendThirdCard = DateTime.Now;
                        contributorComp.SendThirdCardBy = User.Identity.Name;

                        contributorComp.PaymentType = paymentType;

                    }
                    else
                    {
                        switch (cardType)
                        {
                            case 1:
                                contributorComp.PaidHelper = includeHelper ? contributor.HelperEmail : "";
                                contributorComp.PaymentType = paymentType;
                                contributorComp.SendFirstCard = DateTime.Now;
                                contributorComp.SendFirstCardBy = User.Identity.Name;
                                break;
                            case 2:
                                contributorComp.PaidHelper = includeHelper ? contributor.HelperEmail : "";
                                contributorComp.PaymentType = paymentType;
                                contributorComp.SendSecondCard = DateTime.Now;
                                contributorComp.SendSecondCardBy = User.Identity.Name;
                                break;
                            default:
                                contributorComp.PaidHelper = includeHelper ? contributor.HelperEmail : "";
                                contributorComp.PaymentType = paymentType;
                                contributorComp.SendThirdCard = DateTime.Now;
                                contributorComp.SendThirdCardBy = User.Identity.Name;
                                break;
                        }
                    }

                    
                    newCompensations.Add(contributorComp);
                }
                //add to gift card list
                //var contributor = _contributorViewRepository.Find(c => c.Id == id).FirstOrDefault();
               
                if (contributor != null)
                {
                    if (paymentType == "check") //we write one check for all 3 gift cards, contributor gets $180, helper gets $90
                    {
                        //need to add first recording date and last recording date for payment by check
                        var recordingList = _recordingRepository.Find(r => r.ContributorId == contributor.Id);

                        var firstRecordingDate = recordingList.Min(r => r.CreateTS);
                        var lastRecordingDate = recordingList.Max(r => r.CreateTS);
                      
                        giftCardsByCheck.Add(new GiftCardByCheckViewModel() 
                            { Amount = 180.00, EmailAddress = contributor.EmailAddress, FirstName = contributor.FirstName, LastName = contributor.LastName,FirstRecordingDate = firstRecordingDate, LastRecordingDate = lastRecordingDate});

                        if (includeHelper)
                        {
                            giftCardsByCheck.Add(new GiftCardByCheckViewModel { Amount = 90.00,  EmailAddress = contributor.HelperEmail, FirstName = contributor.HelperFirstName, LastName = contributor.HelperLastName, FirstRecordingDate = firstRecordingDate, LastRecordingDate = lastRecordingDate });
                        }
                    }
                    else
                    {
                        giftCards.Add(new GiftCardViewModel { Amount = 60.00, Delay = 0, EmailAddress = contributor.EmailAddress });

                        if (includeHelper)
                        {
                            giftCards.Add(new GiftCardViewModel { Amount = 30.00, Delay = 0, EmailAddress = contributor.HelperEmail });
                        }
                    }
                }
            }

            if (newCompensations.Count > 0)
            {
                _contributorCompensationRepository.AddRange(newCompensations);
            }

            if (paymentType == "check")
            {
                if (giftCardsByCheck.Count > 0)
                {
                    ////save the file to server folder
                    var basePath = _configuration["AppSettings:UploadFileFolder"] + "\\Download";
                    var fullPath = Path.Combine(basePath, fileName);

                    using var writer = new StreamWriter(fullPath);
                    using var csv = new CsvWriter(writer,
                        new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true });
                    csv.WriteRecords(giftCardsByCheck);
                    return Json(new { Success = true, Message = fileName });
                }
            }
            else
            {
                if (giftCards.Count > 0)
                {
                    ////save the file to server folder
                    var basePath = _configuration["AppSettings:UploadFileFolder"] + "\\Download";
                    var fullPath = Path.Combine(basePath, fileName);

                    using var writer = new StreamWriter(fullPath);
                    using var csv = new CsvWriter(writer,
                        new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false });
                    csv.WriteRecords(giftCards);
                    return Json(new { Success = true, Message = fileName });
                }
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

            IQueryable<ContributorCompensationHistory> compensationHistory;

            if (!string.IsNullOrEmpty(searchValue))
            {
                 compensationHistory = _contributorCompensationHistoryRepository.GetAll()
                    .Where(c => c.ContributorId.ToString().Contains(searchValue) || c.LastName.Contains(searchValue) || c.FirstName.Contains(searchValue)
                                || c.EmailAddress.Contains(searchValue) || c.HelperFirstName.Contains(searchValue) || c.HelperLastName.Contains(searchValue) || c.HelperEmail.Contains(searchValue));
            }
            else
            {
                 compensationHistory = _contributorCompensationHistoryRepository.GetAll();

            }

           
            var recordsTotal = compensationHistory.Count();
            compensationHistory = DynamicSortingExtensions<ContributorCompensationHistory>.SetOrderByDynamic(compensationHistory, Request.Form);
            compensationHistory = compensationHistory.Skip(skip).Take(pageSize);


            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = compensationHistory });

        }

        public IActionResult ViewAnnotationProgress()
        {

            DateTime endDate = DateTime.Today;
            DateTime startDate = endDate.AddDays(-7);
            ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.ToString("yyyy-MM-dd");
            return View();
        }
      

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
           
            var annotatorList = annotators.Select(p => new User()
            {
                Id = p.User.Id,
                FirstName = p.User.FirstName,
                LastName = p.User.LastName,
                NetId = p.User.NetId
            }).Distinct();
            annotatorList = DynamicSortingExtensions<User>.SetOrderByDynamic(annotatorList, Request.Form);
            annotatorList = annotatorList.Skip(skip).Take(pageSize);
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
                var contributors = _contributorAssignedAnnotatorRepository.Find(a => a.UserId == annotatorId)
                    .ToList();
                //var contributorIds = _contributorAssignedAnnotatorRepository.Find(a => a.UserId==annotatorId)
                //    .ToList().Select(a => a.ContributorId);
                var contributorIds = contributors.Select(a => a.ContributorId);

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
                        ReportDate = startDate,
                        EtiologyName = _contributorViewRepository.Find(c=>c.Id==group.Key.ContributorId).FirstOrDefault()!.EtiologyName

                    })
                    .ToList();
                list.Add(temp);
                startDate = startDate.AddDays(1);
            }

            return PartialView("_AnnotatorsWorkingSpeechFiles", list);

        }

        public IActionResult ViewContributorAssignedList()
        {
            ViewBag.DownLoadBasePath = _configuration["AppSettings:UploadFileFolder"] + "\\Download"; 
            return View();
        }

        [HttpPost]
        public ActionResult LoadContributorAssignedList()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            //var assignedList = _contributorAssignedListRepository.GetAll().GroupBy(l => new { l.ContributorId, l.FirstName, l.LastName, l.ListId})
            //    .Select(l => 
            //        new ContributorAssignedList
            //        {
            //            ContributorId = l.Key.ContributorId, 
            //            FirstName = l.Key.FirstName,
            //            LastName = l.Key.LastName,
            //            ListId = l.Key.ListId
            //        }); 
            var assignedList = _contributorAssignedListRepository.GetAll()
                .Where(l =>l.FirstName.Contains(searchValue) || l.LastName.Contains(searchValue) || l.ContributorId.ToString().Contains(searchValue)
                || l.BlockName.Contains(searchValue) || l.ListName.ToLower().Contains(searchValue.ToLower()));


            var recordsTotal = assignedList.Count();

            assignedList = DynamicSortingExtensions<ContributorAssignedList>.SetOrderByDynamic(assignedList, Request.Form);
            assignedList = assignedList.Skip(skip).Take(pageSize);
            return Json(new { draw, data = assignedList, recordsFiltered = recordsTotal, recordsTotal = recordsTotal });

        }
        //PublishedRecordingsByEtiology
        public IActionResult PublishedRecordingsByEtiology()
        {
            return View();
        }

        public ActionResult LoadPublishedRecordingsByEtiology(int annotatorId)
        {
            var publishedRecordingsByEtiology = _numberOfRecordingByEtiologyRepository.GetAll().AsEnumerable().GroupBy(r => r.EtiologyId)
                .Select(r => new
                {
                    EtiologyId = r.Key,
                    NumberOfRecordings = r.Count()
                }).ToList();

            var spontaneousSpeechRecordingsByEtiology = _numberOfRecordingByEtiologyRepository.Find(r=>r.CategoryId==4).AsEnumerable().GroupBy(r => r.EtiologyId)
                .Select(r => new
                {
                    EtiologyId = r.Key,
                    NumberOfRecordings = r.Count()
                }).ToList();

            List<PublishedRecordingsByEtiologyViewModel> data = new List<PublishedRecordingsByEtiologyViewModel>();

            foreach (var r in publishedRecordingsByEtiology)
            {
                var temp = new PublishedRecordingsByEtiologyViewModel
                {
                    EtiologyId = r.EtiologyId,
                    NumberOfRecordings = r.NumberOfRecordings
                };
              
                var spontaneousSpeechRecordings = spontaneousSpeechRecordingsByEtiology
                    .Find(s => s.EtiologyId == r.EtiologyId);
                if (spontaneousSpeechRecordings != null)
                {
                    temp.SpontaneousSpeechRecordings = spontaneousSpeechRecordingsByEtiology
                        .Find(s => s.EtiologyId == r.EtiologyId)!.NumberOfRecordings;

                }

                data.Add(temp);
            }

            return data.Any() ? Json(new { Counter = data.Count, recordings = data }) : Json(new { Counter = 0 });


            //return publishedRecordingsByEtiology.Any() ? Json(new { Counter = publishedRecordingsByEtiology.Count, recordings = publishedRecordingsByEtiology }) : Json(new { Counter = 0 });
        }

        [HttpPost]
        public JsonResult ExportContributorAssignedListToExcel()
        {
            //string basePath = "";
            var dataTableToExcel = new DataTableToExcel();
            var assignedList = _contributorAssignedListRepository.GetAll()
                .Select(c=> new{ c.ContributorId, c.ListName, c.BlockName }).ToList();
            //.Select(c => new { c.FirstName, c.LastName, c.EmailAddress,c.HelperFirstName,c.HelperLastName, c.HelperEmail }).ToList();
            var fileName = "ContributorAssignedList_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".xls";


            ////save the file to server DownloadReports folder
            var basePath = _configuration["AppSettings:UploadFileFolder"] + "\\Download";
            var fullPath = Path.Combine(basePath, fileName);


            DataTable dataSet = dataTableToExcel.ToDataTable(assignedList);

            using (var exportData = new MemoryStream())
            {
                dataTableToExcel.DataToExcel(dataSet, exportData);
                var file = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                exportData.WriteTo(file);
                file.Close();
            }

            return Json(new { Success = true, Message = fileName });
        }

        public IActionResult RecruitingRecordingProgress()
        {
            var report = new RecruitingRecordingProgressViewModel();
            var etiologies = _etiologyViewRepository.GetAll().OrderBy(e=>e.DisplayOrderForReports).ToList();
            report.Etiologies = etiologies;

            var status = _contributorStatusRepository.GetAll().Where(s=> s.Id!=5).Include(s=>s.ContributorSubStatus).OrderBy(s=>s.DisplayOrderForReports).ToList();
            
            report.ContributorStatus = status;
            var allContributors = _contributorViewRepository.GetAll().ToList();
            report.Contributors = allContributors;
            return View(report);
        }

    }
}
