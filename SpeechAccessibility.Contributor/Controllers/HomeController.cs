/*
 MIT License

 Copyright (c) 2022 The Speech Accessibility Project,
 The Beckman Institute,
 University of Illinois, Urbana-Champaign

 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:

 The above copyright notice and this permission notice shall be included in all
 copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpeechAccessibility.Data;
using SpeechAccessibility.Data.Entities;
using SpeechAccessibility.Models;
using SpeechAccessibility.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using System.Speech.Synthesis;
using Prompt = SpeechAccessibility.Models.Prompt;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.VisualBasic;
using System.Drawing;
using System.Numerics;
using System.Security.Policy;
using System.Speech.Recognition;
using Microsoft.EntityFrameworkCore;

namespace SpeechAccessibility.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IdentityContext _identityContext;
        private readonly RecordingContext _recordingContext;
        private readonly IMailService _emailSender;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger, IdentityContext identityContext, RecordingContext recordingContext, IMailService emailSender, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration config)
        {
            _logger = logger;
            _identityContext = identityContext;
            _recordingContext = recordingContext;
            _emailSender = emailSender;
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        public IActionResult Index()
        {
            DateTime currentDate = DateTime.Now;
            ViewBag.MaintenanceMessage = _recordingContext.Maintenance.Where(m => m.StartDate <= currentDate && m.EndDate >= currentDate).Where(m => m.ContributorInd == true).Select(m => m.Message).FirstOrDefault();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public void LogError(IFormCollection form)
        {
            string errorMessage = form["error"];
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string fileLocation = _config["ErrorLocation"] + date + "SpeechAccessibility.txt";

            Directory.CreateDirectory(Path.GetDirectoryName(fileLocation));
            using (StreamWriter writer = new StreamWriter(fileLocation, true))
            {
                string error = DateTime.Now.ToString() + " " + errorMessage;
                writer.WriteLine(error);
                writer.Close();
            }
        }

        [HttpPost]
        public void SaveRecording(IFormCollection form)
        {
            try
            {
                SaveToDB(form);
                SaveToDisk(form);

            }
            catch (Exception e)
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                string fileLocation = _config["ErrorLocation"] + date + "SpeechAccessibility.txt";
                Guid contributorId = new Guid(form["contributorId"]);

                Directory.CreateDirectory(Path.GetDirectoryName(fileLocation));
                using (StreamWriter writer = new StreamWriter(fileLocation, true))
                {
                    string error = DateTime.Now.ToString() + " " + contributorId + " " + e;
                    writer.WriteLine(error);
                    writer.Close();
                }
                throw;
            }
        }

        private void SaveToDB(IFormCollection form)
        {
            try
            {
                IFormFile file = form.Files[0];
                Guid contributorId = new Guid(form["contributorId"]);
                int promptId = Int32.Parse(form["promptId"]);
                string categoryId = form["categoryId"];
                string fileName = Path.ChangeExtension(file.FileName, ".wav");
                int retryCount = Int32.Parse(form["retryCount"]);
                int blockId = Int32.Parse(form["blockId"]);
                string clientStartDate = form["clientStartDate"];
                string clientEndDate = form["clientEndDate"];
                string transcript = _recordingContext.Prompt.Where(p => p.Id == promptId).Select(p => p.Transcript).First();

                Prompt prompt = new Prompt
                {
                    Id = promptId

                };


                Block block = null;

                if (blockId != 0)
                {
                    block = new Block
                    {
                        Id = blockId
                    };

                }
                Recording recording = new Recording
                {
                    FileName = fileName,
                    OriginalPrompt = prompt,
                    ContributorId = contributorId,
                    Status = new RecordingStatus { Id = 1 },
                    RetryCount = retryCount,
                    Block = block,
                    ModifiedTranscript = transcript,
                    ClientStartTS = clientStartDate,
                    ClientEndTS = clientEndDate

                };

                Recording existingRecording = _recordingContext.Recording.Where(r => r.FileName == fileName).FirstOrDefault();

                //If there isn't an existing recording, add it
                if (existingRecording == null)
                {
                    _recordingContext.Recording.Add(recording);
                    _recordingContext.RecordingStatus.Remove(recording.Status);
                    _recordingContext.Prompt.Remove(recording.OriginalPrompt);
                    if (recording.Block != null)
                    {
                        _recordingContext.Block.Remove(recording.Block);
                    }

                    _recordingContext.SaveChanges();

                }
                //Update retry count of existing recording
                else
                {
                    existingRecording.RetryCount = retryCount;
                    _recordingContext.SaveChanges();
                }

            }
            catch (Exception e)
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                string fileLocation = _config["ErrorLocation"] + date + "SpeechAccessibility.txt";

                Directory.CreateDirectory(Path.GetDirectoryName(fileLocation));
                using (StreamWriter writer = new StreamWriter(fileLocation, true))
                {
                    string error = DateTime.Now.ToString() + e;
                    writer.WriteLine(error);
                    writer.Close();
                }

            }

        }

        private void SaveToDisk(IFormCollection form)
        {
            IFormFile file = form.Files[0];
            Guid contributorId = new Guid(form["contributorId"]);
            int blockId = Int32.Parse(form["blockId"]);

            string fileLocation = _config["FileLocation"];
            string contributorDirectory = _config["FileLocation"] + "\\" + contributorId;
            string blockDirectory = _config["FileLocation"] + "\\" + contributorId + "\\" + blockId;

            try
            {
                CreateSubdirectories(contributorId, blockId, fileLocation, contributorDirectory, blockDirectory);

                string rawFileLocation = blockDirectory + "\\raw";

                string rawFullPath = Path.Combine(rawFileLocation, file.FileName);

                using (Stream fileStream = new FileStream(rawFullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    file.CopyTo(fileStream);

                    fileStream.Close();

                }
                CopyFile(file, blockDirectory, rawFullPath);

            }
            catch
            {

                throw;
            }

        }

        public void CopyFile(IFormFile file, string blockDirectory, string rawFullPath)
        {
            string fileName = Path.ChangeExtension(rawFullPath, ".wav");

            string modifiedFileLocation = blockDirectory + "\\modified";

            string modifiedFileName = Path.Combine(modifiedFileLocation, file.FileName);

            if (System.IO.File.Exists(modifiedFileName))
            {
                System.IO.File.Delete(modifiedFileName);
            }

            System.IO.File.Copy(fileName, modifiedFileName);
        }

        private static void CreateSubdirectories(Guid contributorId, int blockId, string fileLocation, string contributorDirectory, string blockDirectory)
        {
            var parentDir = new DirectoryInfo(fileLocation);

            parentDir.CreateSubdirectory(contributorId.ToString());

            var contributorDir = new DirectoryInfo(contributorDirectory);

            contributorDir.CreateSubdirectory(blockId.ToString());

            var blockDir = new DirectoryInfo(blockDirectory);

            blockDir.CreateSubdirectory("raw");
            blockDir.CreateSubdirectory("modified");
        }

        public IActionResult ApprovalRequired(int etiologyId)
        {
            ViewBag.Etiology = etiologyId;
            return View("ApprovalRequired");
        }

        [Authorize]
        [HttpGet]
        public IActionResult RecordPrompt(bool displayedMessageForCurrentBlock)
        {
            RecordPromptModel model = new RecordPromptModel();

            Prompt prompt = new Prompt();
            Category category = new Category();
            SubCategory subCategory = new SubCategory();
            Contributor contributor = getCurrentContributor();

            Guid contributorId = contributor.Id;
            int statusId = contributor.StatusId;
            string contributorStatus = _identityContext.ContributorStatus.Where(c => c.Id == statusId).Select(c => c.Name).FirstOrDefault();
            int retryCount = 0;
            int retryMax = Int32.Parse(_config["RetryMax"]);

            //Count the number or recordings that the current contributor has completed, excluding section 1
            int totalCount = _recordingContext.Prompt.Where(p => _recordingContext.Recording.Where(r => r.ContributorId == contributorId).Select(r => r.OriginalPrompt.Id).Contains(p.Id)).Where(p => p.Category.Id != 1).Count();

            int assessmentMax = _recordingContext.Prompt.Where(p => p.Category.Id == 1 && p.Active == "Yes").Count();
            int assessmentCount = _recordingContext.Recording.Where(r => r.ContributorId == contributorId).Select(r => r.OriginalPrompt).Where(p => p.Category.Id == 1).Count();

            int dysarthriaAssessmentMax = _recordingContext.Prompt.Where(p => p.Category.Id == 1 && p.SubCategory.Id == 3 && p.Active == "Yes").Count();
            int dysarthriaAssessmentCount = _recordingContext.Prompt.Where(p => _recordingContext.Recording.Where(r => r.ContributorId == contributorId).Select(r => r.OriginalPrompt.Id).Contains(p.Id)).Where(p => p.Category.Id == 1 && p.SubCategory.Id == 3).Count();

            int currentBlockOfPromptsCount = 0;
            int blockId = 0;
            int phonationPromptCount = 0;

            string developerMode = _config["DeveloperMode"];

            Recording lastRecording = _recordingContext.Recording.Where(r => r.ContributorId == contributorId).AsEnumerable().LastOrDefault();

            //Approval is required after registration
            if ("Approved".Equals(contributorStatus) || "Yes".Equals(developerMode))
            {
                int consentCount = _identityContext.Consent.Where(c => c.Contributor.Id == contributorId && c.ConsentType != "Caregiver").Count();
                int assentCount = _identityContext.Assent.Where(a => a.ContributorId == contributorId).Count();

                //Only route to the recording page after they have completed the consent page
                if (consentCount > 0 || (contributor.Etiology.Id==4 && assentCount>0))
                {
                    int caregiverConsentCount = _identityContext.Consent.Where(c => c.Contributor.Id == contributorId && c.ConsentType == "Caregiver").Count();
                    string helperInd = _identityContext.Contributor.Where(c => c.Id == contributorId).Select(c => c.HelperInd).First();

                    if (contributor.Etiology.Id == 2)
                    {
                        int legalGuardianCount = _identityContext.LegalGuardian.Where(l => l.ContributorId == contributorId).Count();
                        
                        if ("Yes".Equals(helperInd) && caregiverConsentCount == 0)
                        {
                            return RedirectToPage("/Account/DSCaregiverConsent", new { area = "Identity" });
                        }

                        if (legalGuardianCount > 0 && assentCount == 0)
                        {
                            return RedirectToPage("/Account/DSAssent", new { area = "Identity" });
                        }
                    }
                    else if (contributor.Etiology.Id == 6)
                    {
                        if ("Yes".Equals(helperInd) && caregiverConsentCount == 0)
                        {
                            return RedirectToPage("/Account/ALSCaregiverConsent", new { area = "Identity" });
                        }
                    }
                    else if (contributor.Etiology.Id == 3)
                    {
                        if ("Yes".Equals(helperInd) && caregiverConsentCount == 0)
                        {
                            return RedirectToPage("/Account/CPCaregiverConsent", new { area = "Identity" });
                        }
                    }
                    else if (contributor.Etiology.Id == 4)
                    {                      
                        if ("Yes".Equals(helperInd) && caregiverConsentCount == 0)
                        {
                            return RedirectToPage("/Account/AphasiaCaregiverConsent", new { area = "Identity" });
                        }
                       
                    }

                    int contributorDetailsCount = _identityContext.ContributorDetails.Where(c => c.Contributor.Id == contributorId).Count();

                    //Display the last prompt they recorded if the max retry count hasn't been met
                    if (lastRecording != null && lastRecording.RetryCount < retryMax)
                    {
                        blockId = _recordingContext.Recording.Where(r => r.Id == lastRecording.Id).Select(r => r.Block.Id).First();
                        prompt = _recordingContext.Recording.Where(r => r.Id == lastRecording.Id).Select(r => r.OriginalPrompt).First();
                        category = _recordingContext.Prompt.Where(p => p.Id == prompt.Id).Select(p => p.Category).FirstOrDefault();
                        retryCount = lastRecording.RetryCount;
                        SubCategory currentSubCategory = _recordingContext.Prompt.Where(p => p.Id == prompt.Id).Select(p => p.SubCategory).FirstOrDefault();

                        if (currentSubCategory != null)
                        {
                            subCategory = currentSubCategory;
                        }
                    }
                    else
                    {
                        blockId = setBlock(contributorId);

                        //If there are no more blocks, route to complete page
                        if (blockId == 0)
                        {

                            return RedirectToAction("CompleteConfirmation");
                        }
                        //Select a prompt from the current block that has not already been recorded by the contributor
                        prompt = _recordingContext.BlockOfPrompts.Where(b => b.Block.Id == blockId).Select(b => b.Prompt).Where(b => !_recordingContext.Recording.Where(r => r.ContributorId == contributorId && r.Block.Id == blockId).Select(r => r.OriginalPrompt.Id).Contains(b.Id)).FirstOrDefault();
                        category = _recordingContext.Prompt.Where(p => p.Id == prompt.Id).Select(p => p.Category).FirstOrDefault();

                        SubCategory currentSubCategory = _recordingContext.Prompt.Where(p => p.Id == prompt.Id).Select(p => p.SubCategory).FirstOrDefault();

                        if (currentSubCategory != null)
                        {
                            subCategory = currentSubCategory;
                        }

                    }

                    currentBlockOfPromptsCount = _recordingContext.Prompt.Where(p => _recordingContext.Recording.Where(r => r.ContributorId == contributorId && r.RetryCount == retryMax && r.Block.Id == blockId).Select(r => r.OriginalPrompt.Id).Contains(p.Id)).Count();

                }
                //Route to the consent page if they haven't provided consent
                else
                {
                    if (contributor.Etiology.Id == 2)
                    {
                        return RedirectToPage("/Account/DSConsent", new { area = "Identity" });
                    }
                    if (contributor.Etiology.Id == 6)
                    {
                        return RedirectToPage("/Account/ALSConsent", new {area="Identity"});
                    }
                    if (contributor.Etiology.Id == 1)
                    {
                        return RedirectToPage("/Account/ConsentLanguage", new { area = "Identity" });
                    }
                    if (contributor.Etiology.Id == 3)
                    {
                        return RedirectToPage("/Account/CPConsent", new { area = "Identity" });
                    }
                    if (contributor.Etiology.Id == 4)
                    {

                       return RedirectToPage("/Account/AphasiaAssent", new { area = "Identity" });          

                    }
                    else
                    {
                        return RedirectToPage("/Account/Consent", new { area = "Identity" });
                    }


                }

            }
            //Route to the ApprovalRequired page if the status isn't Approved
            else if ("New".Equals(contributorStatus) || "Non-Responsive".Equals(contributorStatus))
            {
                ViewBag.Etiology = contributor.Etiology.Id;
                return View("ApprovalRequired");
            }
            else if (("Denied".Equals(contributorStatus)))
            {
                return View("Denied");
            }

            prompt.Category = category;
            prompt.SubCategory = subCategory;
            model.prompt = prompt;
            model.count = totalCount + 1;
            model.contributorId = contributorId;
            //There are three options for the last assessment prompt. We only want to count this as one question
            model.assessmentMax = assessmentMax - 2;
            model.assessmentCount = assessmentCount + 1;
            model.digitalCommandMax = _recordingContext.BlockOfPrompts.Where(p => p.Block.Id == blockId && p.Category.Id == 2).Count();
            model.novelSentenceMax = _recordingContext.BlockOfPrompts.Where(p => p.Block.Id == blockId && p.Category.Id == 3).Count();
            model.uaPromptMax = Int32.Parse(_config["CPUAPromptMax"]);
            model.fiveKPromptMax = Int32.Parse(_config["CP5KPromptMax"]);
            model.openEndedMax = _recordingContext.BlockOfPrompts.Where(p => p.Block.Id == blockId && p.Category.Id == 4).Count();
            model.currentBlockOfPromptsCount = currentBlockOfPromptsCount;
            model.totalBlockCount = _recordingContext.Recording.Where(r => r.ContributorId == contributorId).Select(r => r.Block).Where(b => b != null).Distinct().Count();
            model.blockId = blockId;
            model.blockMax = _recordingContext.BlockOfPrompts.Where(p => p.Block.Id == blockId).Count();
            model.retryCount = retryCount;
            model.phonationPromptCount = phonationPromptCount;
            model.etiologyId = contributor.Etiology.Id;
            model.promptCategoryId = _identityContext.Contributor.Where(c => c.Id == contributorId).Select(c => c.PromptCategoryId).FirstOrDefault();

            HttpContext.Response.Cookies.Append("phonationPromptCount", phonationPromptCount.ToString());

            string url = contributorId + prompt.Id.ToString();
            HttpContext.Response.Cookies.Append("url", "/Home/SaveRecording");

            //For CP we want to display the open ended instructions on a separate page.
            //It should route to this page at the beginning of the open ended section for each block
            if((((model.etiologyId == 3 ||model.etiologyId==4) && model.promptCategoryId!=5 && category.Id == 4)) && !displayedMessageForCurrentBlock)
            {
                if (currentBlockOfPromptsCount == model.digitalCommandMax)
                {
                    return RedirectToPage("/Account/OpenEndedInstructions", new { area = "Identity", totalBlockCount = model.totalBlockCount });
                }
            }

            return View(model);

        }

        private Contributor getCurrentContributor()
        {
            Task<IdentityUser> current_User = _userManager.GetUserAsync(User);
            IdentityUser user = current_User.Result;
            string id = user.Id;

            //Retrieve the contributor that is linked to the signed in user ID
            Contributor contributor = _identityContext.Contributor
               .Where(o => o.IdentityUser.Id == id)
               .FirstOrDefault();

            Etiology etiology = _identityContext.Contributor
               .Where(o => o.IdentityUser.Id == id)
               .Select(o => o.Etiology)
               .FirstOrDefault();
            contributor.Etiology = etiology;
            return contributor;
        }

        private int setBlock(Guid contributorId)
        {
            int numberOfAssignedBlocks = _recordingContext.ContributorAssignedBlock.Where(r => r.ContributorId == contributorId).Count();
            int blockId = 1;

            if (numberOfAssignedBlocks != 0)
            {
                //Get the last assigned block id
                blockId = _recordingContext.ContributorAssignedBlock.Where(r => r.ContributorId == contributorId).OrderBy(r => r.CreateTS).Select(r => r.Block.Id).LastOrDefault();

                int blockCount = _recordingContext.Recording.Where(r => r.ContributorId == contributorId && r.Block.Id == blockId).Count();
                int blockMax = _recordingContext.BlockOfPrompts.Where(p => p.Block.Id == blockId).Count();

                //if the last block is complete, set a new block
                if (blockCount == blockMax)
                {
                    Block newBlock = createAndAssignBlock(contributorId);

                    if (newBlock == null)
                    {
                        return 0;
                    }

                    blockId = newBlock.Id;
                }
            }
            //If no blocks have been assigned, create one 
            else
            {
                Block newBlock = createAndAssignBlock(contributorId);

                if (newBlock == null)
                {
                    return 0;
                }

                blockId = newBlock.Id;

            }

            return blockId;
        }


        private Block createAndAssignBlock(Guid contributorId)
        {
           
            int assignedBlockCount = _recordingContext.ContributorAssignedBlock.Where(c => c.ContributorId == contributorId).Count();
            int blocksToComplete = Int32.Parse(_config["BlocksToComplete"]);
           
            if (assignedBlockCount >= blocksToComplete)
            {
                return null;
            }

            int etiologyId = _identityContext.Contributor.Where(c => c.Id == contributorId).Select(c => c.Etiology.Id).FirstOrDefault();

            int promptCategoryId, digitalCommandMax, novelSentenceMax, openEndedPromptMax, proceduralPromptMax, uaPromptMax, fivekPromptMax;

            setPromptMax(contributorId, etiologyId, out promptCategoryId, out digitalCommandMax, out novelSentenceMax, out openEndedPromptMax, out proceduralPromptMax, out uaPromptMax, out fivekPromptMax);


            string description = (assignedBlockCount + 1).ToString();

            Block block = new Block
            {
                Active = "Yes",
                Description = description
            };
            _recordingContext.Block.Add(block);
            //_recordingContext.SaveChanges();

            List<Prompt> digitalCommandList = new List<Prompt>();
            List<Prompt> novelSentenceList = new List<Prompt>();
            List<Prompt> openEndedPromptList = new List<Prompt>();
            List<Prompt> proceduralPromptList = new List<Prompt>();
            List<Prompt> uaPromptList = new List<Prompt>();
            List<Prompt> fivekPromptList = new List<Prompt>();

            int blockMasterId = setBlockMasterId(etiologyId, promptCategoryId);

            //The first and last block should contain the same prompts in a random order
            if (assignedBlockCount == (blocksToComplete - 1) || assignedBlockCount == 0)
            {
                assignPromptsForBlock1And10(etiologyId, promptCategoryId,ref digitalCommandList, ref novelSentenceList, ref openEndedPromptList, ref proceduralPromptList, ref uaPromptList, ref fivekPromptList, blockMasterId);

            }
            else
            {
                assignPromptsForBlock2To9(contributorId, etiologyId, assignedBlockCount, promptCategoryId, digitalCommandMax, novelSentenceMax, openEndedPromptMax, proceduralPromptMax, uaPromptMax, fivekPromptMax, block, ref novelSentenceList, ref digitalCommandList, ref openEndedPromptList, ref proceduralPromptList, ref uaPromptList, ref fivekPromptList);

            }

            foreach (Prompt prompt in digitalCommandList)
            {
                addBlockOfPrompts(block, prompt, 2);
            }

            foreach (Prompt prompt in novelSentenceList)
            {
                addBlockOfPrompts(block, prompt, 3);
            }

            foreach (Prompt prompt in openEndedPromptList)
            {
                addBlockOfPrompts(block, prompt, 4);
            }

            foreach (Prompt prompt in proceduralPromptList)
            {
                addBlockOfPrompts(block, prompt, 4);
            }
            foreach (Prompt prompt in uaPromptList)
            {
                addBlockOfPrompts(block, prompt, 5);
            }

            foreach (Prompt prompt in fivekPromptList)
            {
                addBlockOfPrompts(block, prompt, 5);
            }


            ContributorAssignedBlock assignedBlock = new ContributorAssignedBlock
            {
                Block = block,
                ContributorId = contributorId,
                InUsed = "Yes"
            };

            //Added this logic to prevent double click from adding the same block number twice
            List<int> assignedBlockIdList = _recordingContext.ContributorAssignedBlock.Where(c => c.ContributorId == contributorId).Select(c => c.Block.Id).ToList();

            Block currentDescriptionBlock = _recordingContext.Block.Where(b => assignedBlockIdList.Contains(b.Id) && b.Description == description).FirstOrDefault();

            //Check if the block description we're trying to add, already exists
            if (currentDescriptionBlock == null)
            {
                _recordingContext.ContributorAssignedBlock.Add(assignedBlock);
                _recordingContext.SaveChanges();
            }
            else
            {
                block = currentDescriptionBlock;
            }
           

            return block;
        }

        private void assignPromptsForBlock2To9(Guid contributorId, int etiologyId, int assignedBlockCount, int promptCategoryId, int digitalCommandMax, int novelSentenceMax, int openEndedPromptMax, int proceduralPromptMax, int uaPromptMax, int fivekPromptMax, Block block, ref List<Prompt> novelSentenceList, ref List<Prompt> digitalCommandList, ref List<Prompt> openEndedPromptList, ref List<Prompt> proceduralPromptList, ref List<Prompt> uaPromptList, ref List<Prompt> fivekPromptList)
        {
            int assignedDigitalCommandListId = 0;
            int assignedSingleWordListId = 0;
            int assignedDigitalCommandBlockCount = _recordingContext.AssignedDigitalCommandBlock.Where(a => a.ContributorId == contributorId).Count();
            bool useExistingDACBlocks = false;

            DateTime newDACTimeStamp = DateTime.Parse(_config["NewDACTimestamp"]);
           
            if (assignedDigitalCommandBlockCount > 0)
            {
                //If the contributor has already been assigned a digital command block, retrieve the Id
                assignedDigitalCommandListId = _recordingContext.AssignedDigitalCommandBlock.Where(a => a.ContributorId == contributorId).Select(a => a.List.Id).FirstOrDefault();

               DateTime firstAssignedDACBlockTimestamp = _recordingContext.AssignedDigitalCommandBlock.Where(a => a.ContributorId == contributorId).OrderBy(a => a.CreateTS).Select(a => a.CreateTS).FirstOrDefault();
               int dateTimeCompare = DateTime.Compare(firstAssignedDACBlockTimestamp, newDACTimeStamp);
                if (dateTimeCompare <= 0)
                {
                    useExistingDACBlocks=true;
                }
            }
            else
            {
                //If the contributor hasn't been assigned a digital command block, assign one
                assignedDigitalCommandListId = assignDigitalCommandBlock(contributorId, etiologyId,promptCategoryId);
            }

            int blockOfDigitalCommandId = 0;


            if (useExistingDACBlocks && (assignedDigitalCommandListId>10 && assignedDigitalCommandListId<21))
            {
                blockOfDigitalCommandId = _recordingContext.BlockOfDigitalCommand.Where(b => b.List.Id == assignedDigitalCommandListId && b.Active == "No").Select(b => b.Id).Skip(assignedBlockCount - 1).First();
                          
            }
            else {
                blockOfDigitalCommandId = _recordingContext.BlockOfDigitalCommand.Where(b => b.List.Id == assignedDigitalCommandListId && b.Active == "Yes").Select(b => b.Id).Skip(assignedBlockCount - 1).First();
            }
            

            List<int> currentEtiologyPromptList = new List<int>();

            currentEtiologyPromptList = _recordingContext.PromptEtiology.Where(e => e.EtiologyId == etiologyId).Select(e => e.PromptId).ToList();       
         
            digitalCommandList = _recordingContext.BlockOfDigitalCommandPrompts.Where(b => b.BlockOfDigitalCommand.Id == blockOfDigitalCommandId).Select(b => b.Prompt).Take(digitalCommandMax).OrderBy(r => Guid.NewGuid()).ToList();

            if (promptCategoryId !=5)
            {
                proceduralPromptList = _recordingContext.Prompt.Where(p => !_recordingContext.Recording.Where(r => r.ContributorId == contributorId).Select(r => r.OriginalPrompt.Id).Contains(p.Id)).Where(p => p.Category.Id == 4 && p.SubCategory.Id == 2 && p.Active == "Yes" && currentEtiologyPromptList.Contains(p.Id)).OrderBy(r => Guid.NewGuid()).Take(proceduralPromptMax).ToList();

            }

            if (etiologyId == 2)
            {
                int blockNumber = Int32.Parse(block.Description);
                assignOpenEndedPrompts(contributorId, openEndedPromptList, currentEtiologyPromptList, blockNumber,etiologyId);
            }
            else if (etiologyId == 3 || etiologyId==4 || (etiologyId==6 && promptCategoryId==5))
            {
              
                if (promptCategoryId != 5)
                {

                    int blockNumber = Int32.Parse(block.Description);
                    assignOpenEndedPrompts(contributorId, openEndedPromptList, currentEtiologyPromptList, blockNumber,etiologyId);
                }
                else
                {
                    assignedSingleWordListId = assignedDigitalCommandListId - 10;
                    int blockOfSingleWordId = 0;
                    blockOfSingleWordId = _recordingContext.BlockOfSingleWords.Where(b => b.List.Id == assignedSingleWordListId && b.Active=="Yes").Select(b => b.Id).Skip(assignedBlockCount - 1).First();
                    uaPromptList = _recordingContext.BlockOfSingleWordPrompts.Where(b => b.BlockOfSingleWordsId == blockOfSingleWordId).Select(b => b.Prompt).OrderBy(r => Guid.NewGuid()).Take(uaPromptMax).ToList();
                    List<int> currentEtiologyFivekList = _recordingContext.Prompt.Where(p => p.Category.Id == 5 && p.SubCategory.Id == 24 && p.Active == "Yes" && currentEtiologyPromptList.Contains(p.Id)).OrderBy(r => Guid.NewGuid()).Select(p=>p.Id).ToList();
                    
                    List<int> assignedFiveKList = _recordingContext.BlockOfPrompts.Where(b=>currentEtiologyFivekList.Contains(b.Prompt.Id)).Select(b=>b.Prompt.Id).ToList();
                    
                    fivekPromptList = _recordingContext.Prompt.Where(p=>currentEtiologyFivekList.Contains(p.Id) && !assignedFiveKList.Contains(p.Id)).OrderBy(r => Guid.NewGuid()).Take(fivekPromptMax).ToList();

                    //If we run out of unique 5k sentences, then we will have to reuse some
                    if (fivekPromptList.Count < fivekPromptMax)
                    {
                        fivekPromptList = _recordingContext.Prompt.Where(p => !_recordingContext.Recording.Where(r => r.ContributorId == contributorId).Select(r => r.OriginalPrompt.Id).Contains(p.Id)).Where(p => p.Category.Id == 5 && p.SubCategory.Id == 24 && p.Active == "Yes" && currentEtiologyPromptList.Contains(p.Id)).OrderBy(r => Guid.NewGuid()).Take(fivekPromptMax).ToList();
                    }
                }
            }
            else
            {
                novelSentenceList = _recordingContext.Prompt.Where(p => !_recordingContext.BlockOfPrompts.Select(b => b.Prompt.Id).Contains(p.Id)).Where(p => p.Category.Id == 3 && p.Active == "Yes" && currentEtiologyPromptList.Contains(p.Id)).OrderBy(r => Guid.NewGuid()).Take(novelSentenceMax).ToList();
                List<int> recordedPromptList = _recordingContext.Recording.Where(r => r.ContributorId == contributorId).Select(r => r.OriginalPrompt.Id).ToList();
                List<int> currentEtiologyOpenEndedList = _recordingContext.Prompt.Where(p => p.Category.Id == 4 && p.SubCategory.Id == 1 && p.Active == "Yes" && currentEtiologyPromptList.Contains(p.Id)).OrderBy(r => Guid.NewGuid()).Select(p => p.Id).ToList();

                openEndedPromptList = _recordingContext.Prompt.Where(p=>currentEtiologyOpenEndedList.Contains(p.Id) && !recordedPromptList.Contains(p.Id)).OrderBy(r => Guid.NewGuid()).Take(openEndedPromptMax).ToList();
                //If we run out of unique novel sentences, then we will have to reuse some
                if (novelSentenceList.Count < novelSentenceMax)
                {
                    novelSentenceList = _recordingContext.Prompt.FromSqlRaw("select p.Id,p.Transcript,p.CategoryId,p.SubCategoryId,p.QuestionType,p.SeverityLevels,p.CreateTS,p.Active,p.UpdateBy,p.UpdateTS from Prompt as p join PromptEtiology as e on p.Id = e.PromptId where e.EtiologyId =" + etiologyId + " and p.Active = 'Yes' and p.CategoryId = 3 and p.Id not in (select r.OriginalPromptId from Recording as r where r.ContributorId = '" + contributorId + "')").OrderBy(r=>Guid.NewGuid()).Take(novelSentenceMax).ToList();
                }
            }


        }

        private void assignPromptsForBlock1And10(int etiologyId, int promptCategoryId, ref List<Prompt> digitalCommandList, ref List<Prompt> novelSentenceList, ref List<Prompt> openEndedPromptList, ref List<Prompt> proceduralPromptList, ref List<Prompt> uaPromptList, ref List<Prompt> fivekPromptList, int blockMasterId)
        {
            digitalCommandList = _recordingContext.BlockMasterOfPrompts.Where(b => b.BlockMaster.Id == blockMasterId && b.Category.Id == 2).Select(b => b.Prompt).OrderBy(r => Guid.NewGuid()).ToList();
            if (etiologyId == 2)
            {
                openEndedPromptList = _recordingContext.BlockMasterOfPrompts.Where(b => b.BlockMaster.Id == blockMasterId && b.Category.Id == 4).Select(b => b.Prompt).Where(p => p.SubCategory.Id != 2).ToList();

            }
            else if (etiologyId == 3)
            {
                if (promptCategoryId != 5)
                {
                    openEndedPromptList = _recordingContext.BlockMasterOfPrompts.Where(b => b.BlockMaster.Id == blockMasterId && b.Category.Id == 4).Select(b => b.Prompt).Where(p => p.SubCategory.Id != 2).ToList();

                }
                else
                {
                    uaPromptList = _recordingContext.BlockMasterOfPrompts.Where(b => b.BlockMaster.Id == blockMasterId && b.Category.Id == 5).Select(b => b.Prompt).Where(p => p.SubCategory.Id != 24).ToList();
                    fivekPromptList = _recordingContext.BlockMasterOfPrompts.Where(b => b.BlockMaster.Id == blockMasterId && b.Category.Id == 5).Select(b => b.Prompt).Where(p => p.SubCategory.Id == 24).ToList();
                }
            }
            else if (etiologyId == 4)
            {
                //Assign open-ended for spontaneous, single words for non-spontaneous
                if (promptCategoryId != 5)
                {
                    openEndedPromptList = _recordingContext.BlockMasterOfPrompts.Where(b => b.BlockMaster.Id == blockMasterId && b.Category.Id == 4).Select(b => b.Prompt).Where(p => p.SubCategory.Id != 2).ToList();
                }
                else
                {
                    uaPromptList = _recordingContext.BlockMasterOfPrompts.Where(b => b.BlockMaster.Id == blockMasterId && b.Category.Id == 5).Select(b => b.Prompt).Where(p => p.SubCategory.Id != 24).ToList();
                    fivekPromptList = _recordingContext.BlockMasterOfPrompts.Where(b => b.BlockMaster.Id == blockMasterId && b.Category.Id == 5).Select(b => b.Prompt).Where(p => p.SubCategory.Id == 24).ToList();
                }
            }
            else if (etiologyId == 6)
            {
                //Assign open-ended for spontaneous, single words for non-spontaneous
                if (promptCategoryId != 5)
                {
                    novelSentenceList = _recordingContext.BlockMasterOfPrompts.Where(b => b.BlockMaster.Id == blockMasterId && b.Category.Id == 3).Select(b => b.Prompt).OrderBy(r => Guid.NewGuid()).ToList();
                    openEndedPromptList = _recordingContext.BlockMasterOfPrompts.Where(b => b.BlockMaster.Id == blockMasterId && b.Category.Id == 4).Select(b => b.Prompt).Where(p => p.SubCategory.Id == 1).OrderBy(r => Guid.NewGuid()).ToList();
                }
                else {
                    uaPromptList = _recordingContext.BlockMasterOfPrompts.Where(b => b.BlockMaster.Id == blockMasterId && b.Category.Id == 5).Select(b => b.Prompt).Where(p => p.SubCategory.Id != 24).ToList();
                    fivekPromptList = _recordingContext.BlockMasterOfPrompts.Where(b => b.BlockMaster.Id == blockMasterId && b.Category.Id == 5).Select(b => b.Prompt).Where(p => p.SubCategory.Id == 24).ToList();

                }
                
            }

            else
            {
                novelSentenceList = _recordingContext.BlockMasterOfPrompts.Where(b => b.BlockMaster.Id == blockMasterId && b.Category.Id == 3).Select(b => b.Prompt).OrderBy(r => Guid.NewGuid()).ToList();
                openEndedPromptList = _recordingContext.BlockMasterOfPrompts.Where(b => b.BlockMaster.Id == blockMasterId && b.Category.Id == 4).Select(b => b.Prompt).Where(p => p.SubCategory.Id == 1).OrderBy(r => Guid.NewGuid()).ToList();

            }
            //The prompt category will be set to 5 for non-verbal participants.
            //We don't want to assign procedural prompts to them
            if (promptCategoryId != 5)
            {
                proceduralPromptList = _recordingContext.BlockMasterOfPrompts.Where(b => b.BlockMaster.Id == blockMasterId && b.Category.Id == 4).Select(b => b.Prompt).Where(p => p.SubCategory.Id == 2).OrderBy(r => Guid.NewGuid()).ToList();

            }

        }

        private static int setBlockMasterId(int etiologyId, int promptCategoryId)
        {
            int blockMasterId = 1;

            if (etiologyId == 2)
            {
                blockMasterId = 2;
            }
            else if (etiologyId == 6)
            {
                //Use same blockMaster as CP for non-spontaneous
                if (promptCategoryId == 5)
                {
                    blockMasterId = 4;
                }
                else
                {
                    blockMasterId = 3;
                }
                
            }
            else if (etiologyId == 3)
            {
                if (promptCategoryId == 5)
                {
                    //Use single word prompts
                    blockMasterId = 4;
                }
                //Use the same prompts as DS
                else
                {
                    blockMasterId = 2;
                }

            }
            else if (etiologyId == 4)
            {
                //Use same blockMaster as CP for non-spontaneous
                if (promptCategoryId == 5)
                {
                    blockMasterId = 4;
                }
                else 
                {
                    blockMasterId = 5;
                }
                
            }

            return blockMasterId;
        }

        private void setPromptMax(Guid contributorId, int etiologyId, out int promptCategoryId, out int digitalCommandMax, out int novelSentenceMax, out int openEndedPromptMax, out int proceduralPromptMax, out int uaPromptMax, out int fivekPromptMax)
        {
            promptCategoryId = _identityContext.Contributor.Where(c => c.Id == contributorId).Select(c => c.PromptCategoryId).FirstOrDefault();
            digitalCommandMax = 0;
            novelSentenceMax = 0;
            openEndedPromptMax = 0;

            proceduralPromptMax = 0;
            uaPromptMax = 0;
            fivekPromptMax = 0;

            if (etiologyId == 2)
            {
                digitalCommandMax = Int32.Parse(_config["DSDigitalCommandMax"]);
                openEndedPromptMax = Int32.Parse(_config["DSOpenEndedPromptMax"]);
                proceduralPromptMax = Int32.Parse(_config["DSProceduralPromptMax"]);
            }
            else if (etiologyId == 3)
            {              
                digitalCommandMax = Int32.Parse(_config["CPDigitalCommandMax"]);
                if (promptCategoryId == 5)
                {
                    uaPromptMax = Int32.Parse(_config["CPUAPromptMax"]);
                    fivekPromptMax = Int32.Parse(_config["CP5KPromptMax"]);
                }
                else
                {
                    openEndedPromptMax = Int32.Parse(_config["CPOpenEndedPromptMax"]);
                    proceduralPromptMax = Int32.Parse(_config["CPProceduralPromptMax"]);
                }
            }
            else if (etiologyId == 4)
            {
            
                if (promptCategoryId == 5)
                {
                    //Use same set of prompts as CP for non-spontaneous path
                    digitalCommandMax = Int32.Parse(_config["CPDigitalCommandMax"]);
                    uaPromptMax = Int32.Parse(_config["CPUAPromptMax"]);
                    fivekPromptMax = Int32.Parse(_config["CP5KPromptMax"]);
                }
                else
                {
                    digitalCommandMax = Int32.Parse(_config["StrokeDigitalCommandMax"]);
                    openEndedPromptMax = Int32.Parse(_config["StrokeOpenEndedPromptMax"]);
                    proceduralPromptMax = Int32.Parse(_config["StrokeProceduralPromptMax"]);
                }


            }
            else if (etiologyId == 6 && promptCategoryId==5)
            {
             //Use same set of prompts as CP for non-spontaneous path
              digitalCommandMax = Int32.Parse(_config["CPDigitalCommandMax"]);
             uaPromptMax = Int32.Parse(_config["CPUAPromptMax"]);
             fivekPromptMax = Int32.Parse(_config["CP5KPromptMax"]);
                             
            }
            else
            {
                digitalCommandMax = Int32.Parse(_config["DefaultDigitalCommandMax"]);
                novelSentenceMax = Int32.Parse(_config["DefaultNovelSentenceMax"]);
                openEndedPromptMax = Int32.Parse(_config["DefaultOpenEndedPromptMax"]);
                proceduralPromptMax = Int32.Parse(_config["DefaultProceduralPromptMax"]);
            }
        }

        private void assignOpenEndedPrompts(Guid contributorId, List<Prompt> openEndedPromptList, List<int> currentEtiologyPromptList, int blockDescription,int etiologyId)
        {
            int numberOfPairPrompts;
            int numberOfSinglePrompts;

            if (etiologyId == 4)
            {
                if (blockDescription > 8)
                {
                    numberOfPairPrompts = 3;
                    numberOfSinglePrompts = 1;
                }
                else
                {
                    numberOfPairPrompts = 2;
                    numberOfSinglePrompts = 3;

                }
            }
            else {
                if (blockDescription > 6)
                {
                    numberOfPairPrompts = 3;
                    numberOfSinglePrompts = 1;
                }
                else
                {
                    numberOfPairPrompts = 2;
                    numberOfSinglePrompts = 3;

                }
            }
            

            List<Prompt> pairPromptList = _recordingContext.Prompt.Where(p => !_recordingContext.Recording.Where(r => r.ContributorId == contributorId).Select(r => r.OriginalPrompt.Id).Contains(p.Id)).Where(p => p.Category.Id == 4 && p.SubCategory.Id == 17 && p.Active == "Yes" && currentEtiologyPromptList.Contains(p.Id)).OrderBy(r => Guid.NewGuid()).Take(numberOfPairPrompts).ToList();

            foreach (Prompt currentPrompt in pairPromptList)
            {
                openEndedPromptList.Add(currentPrompt);
                int followupPromptId = _recordingContext.FollowupPrompt.Where(f => f.InitialPromptId == currentPrompt.Id).Select(f => f.FollowupPromptId).FirstOrDefault();
                if (followupPromptId > 0)
                {
                    Prompt followupPrompt = _recordingContext.Prompt.Where(p => p.Id == followupPromptId).First();
                    openEndedPromptList.Add(followupPrompt);
                }
            }

            List<Prompt> singleOpenEndedPromptList = _recordingContext.Prompt.Where(p => !_recordingContext.Recording.Where(r => r.ContributorId == contributorId).Select(r => r.OriginalPrompt.Id).Contains(p.Id)).Where(p => p.Category.Id == 4 && p.SubCategory.Id == 1 && p.Active == "Yes" && currentEtiologyPromptList.Contains(p.Id)).OrderBy(r => Guid.NewGuid()).Take(numberOfSinglePrompts).ToList();


            openEndedPromptList.AddRange(singleOpenEndedPromptList);
        }

        private int assignDigitalCommandBlock(Guid contributorId, int etiologyId,int promptCategoryId)
        {

            int lastAssignedDigitalCommandBlockId = 0;
            int numberOfAssignedDigitalCommandBlocks = 0;

            if (etiologyId == 2 || etiologyId == 3)
            {
                List<Guid> currentEtiologyContributorList = _identityContext.Contributor.Where(c => c.Etiology.Id == etiologyId).Select(c => c.Id).ToList();
                lastAssignedDigitalCommandBlockId = 10;
                numberOfAssignedDigitalCommandBlocks = _recordingContext.AssignedDigitalCommandBlock.Where(a => a.List.Id > 10).Where(a => currentEtiologyContributorList.Contains(a.ContributorId)).Count();
                if (numberOfAssignedDigitalCommandBlocks > 0)
                {
                    lastAssignedDigitalCommandBlockId = _recordingContext.AssignedDigitalCommandBlock.Where(a => a.List.Id > 10).Where(a => currentEtiologyContributorList.Contains(a.ContributorId)).OrderBy(a => a.CreateTS).Select(a => a.List.Id).LastOrDefault();
                }
            }
            else if (etiologyId == 4)
            {
                if (promptCategoryId != 5)
                {
                    lastAssignedDigitalCommandBlockId = 20;
                    numberOfAssignedDigitalCommandBlocks = _recordingContext.AssignedDigitalCommandBlock.Where(a => a.List.Id > 20).Count();
                    if (numberOfAssignedDigitalCommandBlocks > 0)
                    {
                        lastAssignedDigitalCommandBlockId = _recordingContext.AssignedDigitalCommandBlock.Where(a => a.List.Id > 20).OrderBy(a => a.CreateTS).Select(a => a.List.Id).LastOrDefault();
                    }
                }
                //Use the same DAC list as CP for non-spontaneous path
                else {
                    List<Guid> currentEtiologyContributorList = _identityContext.Contributor.Where(c => c.Etiology.Id == etiologyId).Select(c => c.Id).ToList();
                    lastAssignedDigitalCommandBlockId = 10;
                    numberOfAssignedDigitalCommandBlocks = _recordingContext.AssignedDigitalCommandBlock.Where(a => a.List.Id > 10 && a.List.Id < 21).Where(a => currentEtiologyContributorList.Contains(a.ContributorId)).Count();
                    if (numberOfAssignedDigitalCommandBlocks > 0)
                    {
                        lastAssignedDigitalCommandBlockId = _recordingContext.AssignedDigitalCommandBlock.Where(a => a.List.Id > 10 && a.List.Id<21).Where(a => currentEtiologyContributorList.Contains(a.ContributorId)).OrderBy(a => a.CreateTS).Select(a => a.List.Id).LastOrDefault();
                    }
                }

            }
            else if (etiologyId==6)
            {
                if (promptCategoryId != 5)
                {
                    numberOfAssignedDigitalCommandBlocks = _recordingContext.AssignedDigitalCommandBlock.Where(a => a.List.Id < 11).Count();
                    if (numberOfAssignedDigitalCommandBlocks > 0)
                    {
                        lastAssignedDigitalCommandBlockId = _recordingContext.AssignedDigitalCommandBlock.Where(a => a.List.Id < 11).OrderBy(a => a.CreateTS).Select(a => a.List.Id).LastOrDefault();
                    }
                }
                //Use the same DAC list as CP for non-spontaneous path
                else
                {
                    List<Guid> currentEtiologyContributorList = _identityContext.Contributor.Where(c => c.Etiology.Id == etiologyId).Select(c => c.Id).ToList();
                    lastAssignedDigitalCommandBlockId = 10;
                    numberOfAssignedDigitalCommandBlocks = _recordingContext.AssignedDigitalCommandBlock.Where(a => a.List.Id > 10).Where(a => currentEtiologyContributorList.Contains(a.ContributorId)).Count();
                    if (numberOfAssignedDigitalCommandBlocks > 0)
                    {
                        lastAssignedDigitalCommandBlockId = _recordingContext.AssignedDigitalCommandBlock.Where(a => a.List.Id > 10).Where(a => currentEtiologyContributorList.Contains(a.ContributorId)).OrderBy(a => a.CreateTS).Select(a => a.List.Id).LastOrDefault();
                    }
                }
            }
            else
            {

                numberOfAssignedDigitalCommandBlocks = _recordingContext.AssignedDigitalCommandBlock.Where(a => a.List.Id < 11).Count();
                if (numberOfAssignedDigitalCommandBlocks > 0)
                {
                    lastAssignedDigitalCommandBlockId = _recordingContext.AssignedDigitalCommandBlock.Where(a => a.List.Id < 11).OrderBy(a => a.CreateTS).Select(a => a.List.Id).LastOrDefault();
                }
            }


            int newCommandBlockId = 0;

            int lastDigitalCommandBlock = Int32.Parse(_config["NumberOfDigitalCommandBlocks"]);

            if (etiologyId == 2 || etiologyId == 3)
            {
                lastDigitalCommandBlock = 20;
            }
            else if (etiologyId == 4)
            {
                if (promptCategoryId != 5)
                {
                    lastDigitalCommandBlock = 30;
                }
                else
                {
                    lastDigitalCommandBlock = 20;
                }
            }
            else if (etiologyId == 6)
            {
                if (promptCategoryId == 5)
                {
                    lastDigitalCommandBlock = 20;
                }
            }

            //Cycle back to list one once we've used all of the lists
            if (lastAssignedDigitalCommandBlockId == lastDigitalCommandBlock)
            {
                newCommandBlockId = 1;

                //List 11-20 are for Down Syndrome and etiologies where they have selected the non-spontaneous prompts
                //List 21-30 are for Stroke with spontaneous prompts
                if (etiologyId == 2 || etiologyId == 3)
                {
                    newCommandBlockId = 11;
                }
                else if (etiologyId == 4)
                {
                    if (promptCategoryId == 5)
                    {
                        newCommandBlockId = 11;
                    }
                    else
                    {
                        newCommandBlockId = 21;
                    }
                }
                else if (etiologyId == 6 && promptCategoryId==5)
                {
                    newCommandBlockId = 11;
                }
            }
            else
            {
                newCommandBlockId = lastAssignedDigitalCommandBlockId + 1;

            }


            List list = new List
            {
                Id = newCommandBlockId
            };

            AssignedDigitalCommandBlock assignedDigitalCommandBlock = new AssignedDigitalCommandBlock
            {
                List = list,
                ContributorId = contributorId
            };

            _recordingContext.AssignedDigitalCommandBlock.Add(assignedDigitalCommandBlock);
            _recordingContext.List.Remove(list);
            _recordingContext.SaveChanges();

            return newCommandBlockId;
        }

        private void addBlockOfPrompts(Block block, Prompt prompt, int categoryId)
        {
            Category category = new Category
            {
                Id = categoryId
            };
            BlockOfPrompts blockOfPrompts = new BlockOfPrompts
            {
                Block = block,
                Prompt = prompt,
                Category = category

            };

            _recordingContext.BlockOfPrompts.Add(blockOfPrompts);
            _recordingContext.Category.Remove(category);
            _recordingContext.SaveChanges();
        }

        [Authorize]
        public IActionResult CompleteConfirmation()
        {
            Contributor contributor = getCurrentContributor();
            Guid contributorId = contributor.Id;

            CompleteConfirmationModel model = new CompleteConfirmationModel();

            model.completedOptionalQuestions = false;

            int contributorDetailsCount = _identityContext.ContributorDetails.Where(c => c.Contributor.Id == contributorId).Count();

            if (contributorDetailsCount > 0)
            {
                model.completedOptionalQuestions = true;
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult RecordPrompt(RecordPromptModel model)
        {
            try
            {
                Guid contributorId = model.contributorId;
                int promptId = model.prompt.Id;
                Recording recording = _recordingContext.Recording.Where(r => r.ContributorId == contributorId).AsEnumerable().LastOrDefault();
                int retryMax = Int32.Parse(_config["RetryMax"]);

                //Once they go to the next prompt, set the retry count to the maximum value to prevent them from being able to rerecord the previous prompt
                recording.RetryCount = retryMax;
                _recordingContext.SaveChanges();

                SendCompletionEmail(contributorId);
            }
            catch (Exception e)
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                string fileLocation = _config["ErrorLocation"] + date + "SpeechAccessibility.txt";

                Directory.CreateDirectory(Path.GetDirectoryName(fileLocation));
                using (StreamWriter writer = new StreamWriter(fileLocation, true))
                {
                    string error = DateTime.Now.ToString() + e;
                    writer.WriteLine(error);
                    writer.Close();
                }
            }

            return RedirectToAction("RecordPrompt");
        }

        private void SendCompletionEmail(Guid contributorId)
        {
            int count = _recordingContext.Recording.Where(r => _recordingContext.Prompt.Where(p => p.Category.Id != 1).Select(p => p.Id).Contains(r.OriginalPrompt.Id)).Where(r => r.ContributorId == contributorId).Count();

            int etiologyId = _identityContext.Contributor.Where(c => c.Id == contributorId).Select(c => c.Etiology.Id).FirstOrDefault();

            int promptCategoryId = _identityContext.Contributor.Where(c => c.Id == contributorId).Select(c => c.PromptCategoryId).FirstOrDefault();

            int totalPromptMax = 450;

            if (etiologyId == 2)
            {
                totalPromptMax = 430;
            }
            else if ((etiologyId == 3 || etiologyId==4) && promptCategoryId != 5)
            {
                totalPromptMax = 430;
            }

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");


            if (count == totalPromptMax)
            {
                Guid userId = (Guid)TempData["contributorId"];

                string emailAddress = _identityContext.Contributor.Where(c => c.Id == userId).Select(c => c.EmailAddress).First();

                string message = "<p style='margin:0in;font-size:15px;font-family:\"Calibri\",sans-serif;'>Congratulations! You&rsquo;ve finished your recordings for the Speech Accessibility Project! We really appreciate all the time you spent participating.</p>\r\n<p style='margin:0in;font-size:15px;font-family:\"Calibri\",sans-serif;'>&nbsp;</p>\r\n<p style='margin:0in;font-size:37px;font-family:\"Calibri Light\",sans-serif;'>Share our project</p>\r\n<p style='margin:0in;font-size:15px;font-family:\"Calibri\",sans-serif;'>We need more participants! Do you know someone who might be qualified and interested in participating in the Speech Accessibility Project? We&rsquo;re recruiting U.S. and Puerto Rican adults who have Parkinson&rsquo;s, ALS, Down syndrome, cerebral palsy, or have had a stroke. We unfortunately cannot include people who live in Illinois, Washington, or Texas.</p>\r\n<p style='margin:0in;font-size:15px;font-family:\"Calibri\",sans-serif;'>Share our information! You can <a href=\"https://speechaccessibilityproject.beckman.illinois.edu/download-our-flyer\">download our flyer</a> on our website. Let your friends know they can <a href=\"https://saa.beckman.illinois.edu/Identity/Account/DiagnosisRegister\">sign up online</a>.</p>\r\n<p style='margin:0in;font-size:15px;font-family:\"Calibri\",sans-serif;'>(Bonus: If you email someone about the project and copy <a href=\"mailto:speechaccessibility@beckman.illinois.edu\">speechaccessibility@beckman.illinois.edu</a>, we&rsquo;ll snail mail you some Speech Accessibility Project stickers!)</p>\r\n<p style='margin:0in;font-size:15px;font-family:\"Calibri\",sans-serif;'>&nbsp;</p>\r\n<p style='margin:0in;font-size:15px;font-family:\"Calibri\",sans-serif;'>Here are answers to questions that people finishing the project often have:</p>\r\n<p style='margin:0in;font-size:15px;font-family:\"Calibri\",sans-serif;'><span style='font-family:\"Calibri Light\",sans-serif;'><span style=\"font-size:37px;\">When will I get my Amazon eCodes?</span></span><br>For your study participation, you will receive up to three $60 installments of Amazon eCodes, with each being sent every one-third of the way through the study. The eCodes are sent to the email address you provided when you signed up. Depending on how quickly you have completed the project, you may have received two eCodes already. It may take several days after you complete the study to receive your final eCode in your email. If you haven&rsquo;t received all three by a week from today, please contact <a href=\"mailto:speechaccessibility@beckman.illinois.edu\">speechaccessibility@beckman.illinois.edu</a> for assistance.</p>\r\n<p style='margin:0in;font-size:15px;font-family:\"Calibri\",sans-serif;'>&nbsp;</p>\r\n<p style='margin:0in;font-size:37px;font-family:\"Calibri Light\",sans-serif;'><strong><span style='font-family:\"Calibri Light\",sans-serif;'>A caregiver assisted me. How will they be compensated?</span></strong></p>\r\n<p style='margin:0in;font-size:15px;font-family:\"Calibri\",sans-serif;'>If a caregiver assisted you, their email address was entered at the start of the study. They will receive up to three $30 eCodes, with each being sent every one-third of the way through the study. If you didn&rsquo;t enter your caregiver at the beginning of the study, please contact your mentor so they can assist you.</p>\r\n<p style='margin:0in;font-size:15px;font-family:\"Calibri\",sans-serif;'>&nbsp;</p>\r\n<p style='margin:0in;font-size:15px;font-family:\"Calibri\",sans-serif;'>Thank you!</p>\r\n<p style='margin:0in;font-size:15px;font-family:\"Calibri\",sans-serif;'>The Speech Accessibility Project Team</p>\r\n<p style='margin:0in;font-size:15px;font-family:\"Calibri\",sans-serif;'>&nbsp;</p>";

                string subject = "You’ve completed the Speech Accessibility Project!";
              
                if (_config["DeveloperMode"].Equals("Yes") || !"Production".Equals(environment))
                {
                    emailAddress = _config["TestEmail"];
                }

                _emailSender.SendEmailAsync(emailAddress, subject, message);

                string helperInd = _identityContext.Contributor.Where(c => c.Id == userId).Select(c => c.HelperInd).First();

                if ("Yes".Equals(helperInd))
                {
                    string helperSubject = "Share our project with a friend!";
                    string helperEmail = _identityContext.Contributor.Where(c => c.Id == userId).Select(c => c.HelperEmail).First();
                    string helperMessage = "<div>    <p>        Hello,    </p>    <p>        Thank you so much for your important work on the        <a href='https://speechaccessibilityproject.beckman.illinois.edu/'>            Speech  Accessibility Project        </a>        . Now that the person you’ve been assisting has finished,  we have one        more request: Would you be willing to send information about the        project to other people who may qualify?    </p>    <p>        Our Big Tech partners are already  using recordings from this project to        improve their speech recognition tools. We’re  also        <a            href='https://speechaccessibilityproject.beckman.illinois.edu/article/2024/04/16/speech-accessibility-project-now-sharing-recordings-data'        >            sharing  the data        </a>        with other universities, companies and nonprofits who propose good        ideas for improved speech accessibility (and agree to respect        participants’  privacy).    </p>    <p>        We need more participants to continue this momentum. We’re  recruiting        U.S. and Puerto Rican adults with Parkinson’s, ALS, cerebral palsy,        Down syndrome, or who have had a stroke.    </p>    <p>        You can        <a            href='https://speechaccessibilityproject.beckman.illinois.edu/download-our-flyer'        >            download  our flyer        </a>        and pass it along. If you copy        <a href='mailto:speechaccessibility@beckman.illinois.edu'>            speechaccessibility@beckman.illinois.edu        </a>        on your email, we’ll mail you some project stickers.    </p>    <p>        Thank you again for your contributions! We appreciate it. As  always, if        you have any questions, please contact us!    </p>    <p>        Sincerely,    </p>    <p>        The Speech Accessibility Project Team    </p>    <br/></div>";
                    if (_config["DeveloperMode"].Equals("Yes") || !"Production".Equals(environment))

                    {
                        helperEmail = _config["TestEmail"];
                    }

                    _emailSender.SendEmailAsync(helperEmail, helperSubject, helperMessage);
                }
            }
               
        }

        [Authorize]
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ContactAsync(ContactViewModel model)
        {
            ContactViewModel contact = new ContactViewModel();
            try
            {
                if (ModelState.IsValid)
                {
                    Guid userId = (Guid)TempData["contributorId"];

                    string emailAddress = _identityContext.Contributor.Where(c => c.Id == userId).Select(c => c.EmailAddress).First();

                    string message = "<div>Message from  " + emailAddress + ":</div><br/><p>" + model.Message + "<p>";

                    string to = _config["AdminEmail"];

                    string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                    if (_config["DeveloperMode"].Equals("Yes") || !"Production".Equals(environment))
                    {
                        to = _config["TestEmail"];
                        string testMessage = "<p><strong>This email was sent in testing mode.</strong></p>";
                        message = testMessage + message;
                    }

                    await _emailSender.SendEmailAsync(to, model.Subject, message);

                    ModelState.Clear();

                    contact.Message = string.Empty;
                    contact.Subject = string.Empty;
                    contact.Status = "Your message was successfully sent.";
                }

            }
            catch (Exception)
            {
                contact.Error = "Email failed to send. Please try again later.";
            }

            return View(contact);
        }

        [Authorize]
        public IActionResult Help()
        {
            return View();
        }

        [Authorize]
        public IActionResult Instructions()
        {
            return View();
        }

        public IActionResult Removal()
        {

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;

            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string fileLocation = _config["ErrorLocation"] + date + "SpeechAccessibility.txt";

            Directory.CreateDirectory(Path.GetDirectoryName(fileLocation));
            using (StreamWriter writer = new StreamWriter(fileLocation, true))
            {
                string error = DateTime.Now.ToString() + Activity.Current?.Id + " " + exceptionHandlerFeature.Error.Message + " " + exceptionHandlerFeature.Error.StackTrace;
                writer.WriteLine(error);
                writer.Close();
            }

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
