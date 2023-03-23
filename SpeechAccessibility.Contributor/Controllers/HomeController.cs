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
using System.Threading.Tasks;
using Xabe.FFmpeg;

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
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveRecording(IFormCollection form)
        {
            try 
            {
                await SaveToDBAsync(form);
                await SaveToDisk(form);

            }
            catch(Exception e)
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
                return RedirectToAction("Error");
            }
             
            return View();
        }

        private async Task SaveToDBAsync(IFormCollection form)
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
                    ModifiedTranscript = transcript

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

                    await _recordingContext.SaveChangesAsync();

                }
                //Update retry count of existing recording
                else
                {
                    existingRecording.RetryCount = retryCount;
                    await _recordingContext.SaveChangesAsync();
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

        private async Task SaveToDisk(IFormCollection form)
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
                    await file.CopyToAsync(fileStream);

                    fileStream.Close();

                }
                await ConvertAndCopyFile(file, blockDirectory, rawFullPath);

            }
            catch {

                throw;
            }               

        }

        public async Task ConvertAndCopyFile(IFormFile file, string blockDirectory, string rawFullPath)
        {
            string convertedFileName = Path.ChangeExtension(rawFullPath, ".wav");
            if (System.IO.File.Exists(convertedFileName))
            {
                System.IO.File.Delete(convertedFileName);
            }

            string ffmpegLocation = _config["FFmpegLocation"];
            FFmpeg.SetExecutablesPath(ffmpegLocation, ffmpegExeutableName: "ffmpeg");
            var conversion = await FFmpeg.Conversions.FromSnippet.Convert(rawFullPath, convertedFileName);
            await conversion.Start();

            string modifiedFileLocation = blockDirectory + "\\modified";

            string modifiedFullPath = Path.Combine(modifiedFileLocation, file.FileName);
            string modifiedFileName = Path.ChangeExtension(modifiedFullPath, ".wav");

            if (System.IO.File.Exists(modifiedFileName))
            {
                System.IO.File.Delete(modifiedFileName);
            }

            System.IO.File.Copy(convertedFileName, modifiedFileName);
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

        [Authorize][HttpGet]
        public IActionResult RecordPrompt()
        {
            RecordPromptModel model = new RecordPromptModel();

            Prompt prompt = new Prompt();
            Category category = new Category();
            SubCategory subCategory = new SubCategory();
            Contributor contributor = getCurrentContributor();

            Guid contributorId = contributor.Id;
            string contributorStatus = _identityContext.Contributor.Where(c => c.Id == contributor.Id).Select(c => c.Status.Name).First();
            int retryCount = 0;
            int retryMax = Int32.Parse(_config["RetryMax"]);

            //Count the number or recordings that the current contributor has completed, excluding section 1
            int totalCount = _recordingContext.Prompt.Where(p => _recordingContext.Recording.Where(r => r.ContributorId == contributorId).Select(r => r.OriginalPrompt.Id).Contains(p.Id)).Where(p => p.Category.Id != 1).Count();

            int assessmentMax = _recordingContext.Prompt.Where(p => p.Category.Id == 1 && p.Active=="Yes").Count();
            int assessmentCount = _recordingContext.Recording.Where(r => r.ContributorId == contributorId).Select(r => r.OriginalPrompt).Where(p => p.Category.Id == 1).Count();

            int dysarthriaAssessmentMax = _recordingContext.Prompt.Where(p => p.Category.Id == 1 && p.SubCategory.Id == 3 && p.Active=="Yes").Count();
            int dysarthriaAssessmentCount = _recordingContext.Prompt.Where(p => _recordingContext.Recording.Where(r => r.ContributorId == contributorId).Select(r => r.OriginalPrompt.Id).Contains(p.Id)).Where(p => p.Category.Id == 1 && p.SubCategory.Id == 3).Count();

            int currentBlockOfPromptsCount = 0;
            int blockId = 0;
            int phonationPromptCount = 0;

            string developerMode = _config["DeveloperMode"];

            if (dysarthriaAssessmentCount > 0 || contributor.ContactLSVT)
            {
                if (dysarthriaAssessmentCount >= dysarthriaAssessmentMax || contributor.ContactLSVT)
                {
                    phonationPromptCount = _recordingContext.Recording.Where(r => r.ContributorId == contributorId).Select(r => r.OriginalPrompt).Where(p => p.SubCategory.Id == 4).Count();

                    //After they complete the dysarthria prompts, they will do the phonation prompt 3 times
                    if (phonationPromptCount == 3 || contributor.ContactLSVT)
                    {
                        //After the phonation prompts are complete, they will do one spontaneous speech prompt
                        int spontaneousPromptCount = _recordingContext.Recording.Where(r => r.ContributorId == contributorId).Select(r => r.OriginalPrompt).Where(p => p.SubCategory.Id == 5).Count();
                        if (spontaneousPromptCount > 0 || contributor.ContactLSVT)
                        {
                            Recording lastRecording = _recordingContext.Recording.Where(r => r.ContributorId == contributorId).AsEnumerable().LastOrDefault();

                            //Approval is required after the first section has been completed
                            if ("Approved".Equals(contributorStatus) || "Yes".Equals(developerMode))
                            {
                                int consentCount = _identityContext.Consent.Where(c => c.Contributor.Id == contributorId).Count();

                                //Only route to the recording page after they have completed the consent page
                                if (consentCount > 0)
                                {
                                    int contributorDetailsCount = _identityContext.ContributorDetails.Where(c => c.Contributor.Id == contributorId).Count();

                                    //Route to the OptionalQuestions page if the contributor details haven't been populated
                                    if (contributorDetailsCount == 0)
                                    {
                                        return RedirectToPage("/Account/OptionalQuestions", new { area = "Identity" });
                                    }

                                    //Display the last prompt they recorded if the max retry count hasn't been met
                                    if (lastRecording != null && lastRecording.RetryCount < retryMax)
                                    {
                                        blockId = _recordingContext.Recording.Where(r => r.Id == lastRecording.Id).Select(r => r.Block.Id).First();
                                        prompt = _recordingContext.Recording.Where(r => r.Id == lastRecording.Id).Select(r => r.OriginalPrompt).First();
                                        category = _recordingContext.Prompt.Where(p => p.Id == prompt.Id).Select(p => p.Category).FirstOrDefault();
                                        retryCount = lastRecording.RetryCount;
                                    }
                                    else
                                    {
                                        blockId = setBlock(contributorId);

                                        //If there are no more blocks, route to complete page
                                        if (blockId == 0)
                                        {
                                            return View("CompleteConfirmation");
                                        }
                                        //Select a prompt from the current block that has not already been recorded by the contributor
                                        prompt = _recordingContext.BlockOfPrompts.Where(p => p.Block.Id == blockId).Select(p => p.Prompt).Where(p => !_recordingContext.Recording.Where(r => r.ContributorId == contributorId && r.Block.Id == blockId).Select(r => r.OriginalPrompt.Id).Contains(p.Id)).FirstOrDefault();
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
                                    return RedirectToPage("/Account/Consent", new { area = "Identity" });

                                }

                            }
                            //Route to the ApprovalRequired page if the status isn't Approved
                            else if ("New".Equals(contributorStatus))
                            {
                                return View("ApprovalRequired");
                            }
                            else if (("Denied".Equals(contributorStatus)))
                            {
                                return View("Denied");
                            }

                        }
                        else
                        {
                            //Select spontaneous speech prompt
                            List<Prompt> spontaneousSpeechList = _recordingContext.Prompt.Where(p => p.Category.Id == 1 && p.SubCategory.Id == 5 && p.Active == "Yes" ).ToList();
                            model.spontaneousSpeechList = spontaneousSpeechList;
                            category.Id = 1;
                            subCategory.Id = 5;

                        }
                    }                  
                    else
                    {
                        //Select phonation prompt
                        prompt = _recordingContext.Prompt.Where(p => !_recordingContext.Recording.Where(r => r.ContributorId == contributorId).Select(r => r.OriginalPrompt.Id).Contains(p.Id)).Where(p => p.Category.Id == 1 && p.SubCategory.Id == 4 && p.Active=="Yes").First();
                        category.Id = 1;
                        subCategory.Id = 4;
                    }

                }
                else
                {
                    //Select a prompt from section one that has not already been recorded by the current contributor
                    prompt = _recordingContext.Prompt.Where(p => !_recordingContext.Recording.Where(r => r.ContributorId == contributorId).Select(r => r.OriginalPrompt.Id).Contains(p.Id)).Where(p => p.Category.Id == 1 && p.SubCategory.Id == 3 && p.Active=="Yes").First();
                    category.Id = 1;
                    subCategory.Id = 3;
                }
            }
            else
            {
                //Select first prompt in section one
                prompt = _recordingContext.Prompt.Where(p => p.Category.Id == 1 && p.SubCategory.Id == 3 && p.Active == "Yes" ).FirstOrDefault();
                category.Id = 1;
                subCategory.Id = 3;
            }


            prompt.Category = category;
            prompt.SubCategory = subCategory;
            model.prompt = prompt;
            model.count = totalCount + 1;
            model.contributorId = contributorId;
            //There are three options for the last assessment prompt. We only want to count this as one question
            model.assessmentMax = assessmentMax - 2;
            model.assessmentCount = assessmentCount + 1;
            model.digitalCommandMax = _recordingContext.BlockOfPrompts.Where(p => p.Block.Id == blockId && p.Category.Id==2).Count();
            model.novelSentenceMax = _recordingContext.BlockOfPrompts.Where(p => p.Block.Id == blockId && p.Category.Id == 3).Count();
            model.currentBlockOfPromptsCount = currentBlockOfPromptsCount;
            model.totalBlockCount = _recordingContext.Recording.Where(r => r.ContributorId == contributorId).Select(r => r.Block).Where(b => b != null).Distinct().Count();
            model.blockId = blockId;
            model.blockMax = _recordingContext.BlockOfPrompts.Where(p => p.Block.Id == blockId).Count();
            model.retryCount = retryCount;
            model.phonationPromptCount = phonationPromptCount;

            HttpContext.Response.Cookies.Append("phonationPromptCount", phonationPromptCount.ToString());

            string url = contributorId + prompt.Id.ToString();
            HttpContext.Response.Cookies.Append("url", "/Home/SaveRecording");

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
            return contributor;
        }

        private int setBlock(Guid contributorId)
        {
            int numberOfAssignedBlocks = _recordingContext.ContributorAssignedBlock.Where(r => r.ContributorId == contributorId).Count();
            int blockId = 1;

            if (numberOfAssignedBlocks != 0)
            {
                //Get the last assigned block id
                blockId = _recordingContext.ContributorAssignedBlock.Where(r => r.ContributorId == contributorId).OrderBy(r=>r.CreateTS).Select(r => r.Block.Id).LastOrDefault();

                int blockCount = _recordingContext.Recording.Where(r => r.ContributorId == contributorId && r.Block.Id == blockId).Count();
                int blockMax = _recordingContext.BlockOfPrompts.Where(p => p.Block.Id == blockId).Count();

                //if the last block is complete, set a new block
                if (blockCount == blockMax)
                {
                    Block newBlock  = createAndAssignBlock(contributorId);

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
            int digitalCommandMax = Int32.Parse(_config["DigitalCommandMax"]);
            int novelSentenceMax = Int32.Parse(_config["NovelSentenceMax"]);
            int openEndedPromptMax = Int32.Parse(_config["OpenEndedPromptMax"]);
            int proceduralPromptMax = Int32.Parse(_config["ProceduralPromptMax"]);

            if (assignedBlockCount >= blocksToComplete)
            {
                return null;
            }

            string description = (assignedBlockCount + 1).ToString();

            Block block = new Block
            {
                Active = "Yes",
                Description = description
            };
            _recordingContext.Block.Add(block);
            _recordingContext.SaveChanges();

            List<Prompt> digitalCommandList = new List<Prompt>();
            List<Prompt> novelSentenceList = new List<Prompt>();
            List<Prompt> openEndedPromptList = new List<Prompt>();
            List<Prompt> proceduralPromptList = new List<Prompt>();

           
            //The first and last block should contain the same prompts in a random order
            if (assignedBlockCount == (blocksToComplete - 1) || assignedBlockCount==0)
            {
                digitalCommandList = _recordingContext.BlockMasterOfPrompts.Where(b => b.BlockMaster.Id == 1 && b.Category.Id == 2).Select(b => b.Prompt).OrderBy(r => Guid.NewGuid()).ToList();
                novelSentenceList = _recordingContext.BlockMasterOfPrompts.Where(b => b.BlockMaster.Id == 1 && b.Category.Id == 3).Select(b => b.Prompt).OrderBy(r => Guid.NewGuid()).ToList();
                openEndedPromptList = _recordingContext.BlockMasterOfPrompts.Where(b => b.BlockMaster.Id == 1 && b.Category.Id == 4).Select(b => b.Prompt).Where(p => p.SubCategory.Id == 1).OrderBy(r => Guid.NewGuid()).ToList();
                proceduralPromptList = _recordingContext.BlockMasterOfPrompts.Where(b => b.BlockMaster.Id == 1 && b.Category.Id == 4).Select(b => b.Prompt).Where(p => p.SubCategory.Id == 2).OrderBy(r => Guid.NewGuid()).ToList();
            }
            else
            {
                int assignedDigitalCommandListId = 0;
                int assignedDigitalCommandBlockCount = _recordingContext.AssignedDigitalCommandBlock.Where(a => a.ContributorId == contributorId).Count();
                if (assignedDigitalCommandBlockCount > 0)
                {
                    //If the contributor has already been assigned a digital command block, retrieve the Id
                    assignedDigitalCommandListId = _recordingContext.AssignedDigitalCommandBlock.Where(a => a.ContributorId == contributorId).Select(a => a.List.Id).FirstOrDefault();
                }
                else
                {
                    //If the contributor hasn't been assigned a digital command block, assign one
                    assignedDigitalCommandListId = assignDigitalCommandBlock(contributorId);
                }
                
                int blockOfDigitalCommandId = 0;

                 blockOfDigitalCommandId = _recordingContext.BlockOfDigitalCommand.Where(b => b.List.Id == assignedDigitalCommandListId).Select(b => b.Id).Skip(assignedBlockCount-1).First();
               
                digitalCommandList = _recordingContext.BlockOfDigitalCommandPrompts.Where(b => b.BlockOfDigitalCommand.Id == blockOfDigitalCommandId).Select(b=>b.Prompt).Take(digitalCommandMax).ToList();                          
                novelSentenceList = _recordingContext.Prompt.Where(p => !_recordingContext.BlockOfPrompts.Select(b => b.Prompt.Id).Contains(p.Id)).Where(p => p.Category.Id == 3 && p.Active == "Yes").OrderBy(r => Guid.NewGuid()).Take(novelSentenceMax).ToList();
                openEndedPromptList = _recordingContext.Prompt.Where(p => !_recordingContext.Recording.Where(r => r.ContributorId == contributorId).Select(r => r.OriginalPrompt.Id).Contains(p.Id)).Where(p => p.Category.Id == 4 && p.SubCategory.Id == 1 && p.Active == "Yes").OrderBy(r => Guid.NewGuid()).Take(openEndedPromptMax).ToList();
                proceduralPromptList = _recordingContext.Prompt.Where(p => !_recordingContext.Recording.Where(r => r.ContributorId == contributorId).Select(r => r.OriginalPrompt.Id).Contains(p.Id)).Where(p => p.Category.Id == 4 && p.SubCategory.Id == 2 && p.Active == "Yes").OrderBy(r => Guid.NewGuid()).Take(proceduralPromptMax).ToList();

                //If we run out of unique novel sentences, then we will have to reuse some
                if (novelSentenceList.Count < novelSentenceMax)
                {
                    novelSentenceList = _recordingContext.Prompt.Where(p => !_recordingContext.Recording.Where(r => r.ContributorId == contributorId).Select(r => r.OriginalPrompt.Id).Contains(p.Id)).Where(p => p.Category.Id == 3 && p.Active == "Yes").OrderBy(r => Guid.NewGuid()).Take(novelSentenceMax).ToList();
                }

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
          

            ContributorAssignedBlock assignedBlock = new ContributorAssignedBlock
            {
                Block= block,
                ContributorId= contributorId,
                InUsed = "Yes"
            };

            _recordingContext.ContributorAssignedBlock.Add(assignedBlock);
            _recordingContext.SaveChanges();

            return block;
        }

        private int assignDigitalCommandBlock(Guid contributorId)
        {
            int numberOfAssignedDigitalCommandBlocks = _recordingContext.AssignedDigitalCommandBlock.Count();

            int lastAssignedDigitalCommandBlockId = 0;
            
            if(numberOfAssignedDigitalCommandBlocks> 0) 
            {
                lastAssignedDigitalCommandBlockId = _recordingContext.AssignedDigitalCommandBlock.OrderBy(b => b.CreateTS).Select(b => b.List.Id).LastOrDefault();
            }

            int newCommandBlockId = 0;
            int numberOfDigitalCommandBlocks = Int32.Parse(_config["NumberOfDigitalCommandBlocks"]);

                //Cycle back to list one once we've used all of the lists
                if (lastAssignedDigitalCommandBlockId == numberOfDigitalCommandBlocks)
                {
                    newCommandBlockId = 1;
                }
                else
                {
                    newCommandBlockId = lastAssignedDigitalCommandBlockId + 1;
                }
            

            List list  = new List
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

        public IActionResult CompleteConfirmation()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RecordPrompt(RecordPromptModel model)
        {
            Guid contributorId = model.contributorId;
            int promptId = model.prompt.Id;
            Recording recording = _recordingContext.Recording.Where(r => r.ContributorId == contributorId).AsEnumerable().LastOrDefault();
            int retryMax = Int32.Parse(_config["RetryMax"]);

            //Once they go to the next prompt, set the retry count to the maximum value to prevent them from being able to rerecord the previous prompt
            recording.RetryCount = retryMax;
             _recordingContext.SaveChanges();
            return RedirectToAction("RecordPrompt");
        }

        [Authorize] [HttpGet]
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
                    string userId = TempData["contributorId"].ToString();

                    string message = "<div>Message from contributor ID,  " + userId + ":</div><br/><p>" + model.Message + "<p>";

                    string to = _config["AdminEmail"];

                    string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                    if (_config["DeveloperMode"].Equals("Yes") || !"Production".Equals(environment))
                    {
                        to = _config["TestEmail"];
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
