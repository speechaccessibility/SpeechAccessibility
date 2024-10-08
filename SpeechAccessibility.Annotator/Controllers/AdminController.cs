﻿using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SpeechAccessibility.Annotator.Extensions;
using SpeechAccessibility.Annotator.Models;
using SpeechAccessibility.Annotator.Services;
using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Controllers
{
    [Authorize(Policy = "AnnotatorAdmin")]
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserSubRoleRepository _userSubRoleRepository;
        private readonly IEtiologyViewRepository _etiologyViewRepository;
        private readonly ISubRoleRepository _subRoleRepository;
        private readonly IEtiologyRepository _etiologyRepository;
        private readonly IGiftCardAmountRepository _giftCardAmountRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IContributorViewRepository _contributorViewRepository;
        private readonly IContributorRepository _contributorRepository;
        private readonly IContributorsPaidByCheckRepository _contributorsPaidByCheckRepository;
        private readonly IHelperNotPaidGiftCardsRepository _helperNotPaidGiftCardsRepository;

        public AdminController(IUserRepository userRepository, IRoleRepository roleRepository, IConfiguration configuration,  IUserSubRoleRepository userSubRoleRepository, IEtiologyViewRepository etiologyRepository, ISubRoleRepository subRoleRepository, IEtiologyRepository etiologyRepository1, IGiftCardAmountRepository giftCardAmountRepository, ICategoryRepository categoryRepository, IContributorViewRepository contributorViewRepository, IContributorRepository contributorRepository, IContributorsPaidByCheckRepository contributorsPaidByCheckRepository, IHelperNotPaidGiftCardsRepository helperNotPaidGiftCardsRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _configuration = configuration;
            _userSubRoleRepository = userSubRoleRepository;
            _etiologyViewRepository = etiologyRepository;
            _subRoleRepository = subRoleRepository;
            _etiologyRepository = etiologyRepository1;
            _giftCardAmountRepository = giftCardAmountRepository;
            _categoryRepository = categoryRepository;
            _contributorViewRepository = contributorViewRepository;
            _contributorRepository = contributorRepository;
            _contributorsPaidByCheckRepository = contributorsPaidByCheckRepository;
            _helperNotPaidGiftCardsRepository = helperNotPaidGiftCardsRepository;
        }

        public IActionResult Index()
        {
           
            return View();
        }
        public ActionResult ManageUsers()
        {
            ViewBag.RoleList = _roleRepository.Find(r => r.InUsed == "Yes").ToList();
            var userVM = new ADMemberViewModel
            {
                // can't add user to SystemAdmin group
                Roles = _roleRepository.Find(r => r.InUsed == "Yes" && r.Id != 1).OrderBy(r => r.Name).Select(a => new SelectListItem()
                {
                    Value = a.Id.ToString() + "_" + Convert.ToInt32(a.HasSubRole).ToString(),
                    Text = a.Name
                })
                    .ToList(),
               
                SubRoles = _etiologyViewRepository.Find(e => e.Active == "Yes").OrderBy(r => r.DisplayOrder).Select(a => new SelectListItem()
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                }).ToList()
            };
            return View(userVM);
        }

        [HttpPost]
        public ActionResult LoadUsers()
        {

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var users = _userRepository.Find(u => u.Active == "Yes").Include(u => u.Role).Where(r => r.Role.Name.Contains(searchValue.Trim()) 
                || r.FirstName.Contains(searchValue.Trim()) || r.LastName.Contains(searchValue.Trim()) || r.NetId.Contains(searchValue.Trim()));
            var recordsTotal = users.Count();

            var personsList = users.Select(p => new ADMemberViewModel()
            {
                Id = p.Id,
                RoleId = p.RoleId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                NetId = p.NetId,
                RoleName = p.Role.Name
            });

            personsList = DynamicSortingExtensions<ADMemberViewModel>.SetOrderByDynamic(personsList, Request.Form);
            personsList = personsList.Skip(skip).Take(pageSize);
            return Json(new { draw, recordsFiltered = recordsTotal, recordsTotal, data = personsList });

        }

        [HttpPost]
        public ActionResult AddUpdateUser(ADMemberViewModel userVM)
        {
            if (userVM.NetId.Trim() == "")
            {
                return Json(new { Success = false, Message = "NetID is required." });
            }

            if (userVM.NetId.Contains("@"))
            {
                return Json(new { Success = false, Message = "Invalid NetID." });
            }

            var userName = userVM.LastName + "," + userVM.FirstName;

            //testing for hacking
            //userVM.Id = 100;

            if (userVM.Id > 0) //update existing person
            {
                var existingUser = _userRepository.Find(p => p.Id == userVM.Id).Include(u=>u.Role)
                   .FirstOrDefault();

                if (existingUser == null)
                    return Json(new { Success = false, Message = "User is not found." });

              
                //if role changed
                bool roleChanged = false;
                var oldADGroupName = "";
                if (existingUser.RoleId != userVM.RoleId)
                {
                    roleChanged = true;
                    oldADGroupName = existingUser.Role.ADGroupName;
                }

                existingUser.RoleId = userVM.RoleId;
                //update AD group
                string error = "";
               
                var newADGroupName = _roleRepository.Find(r => r.Id == userVM.RoleId).FirstOrDefault().ADGroupName;

                if (roleChanged)
                {
                    //remove from old AD group
                    error = ActiveDirectoryService.RemoveMemberFromADGroup(userVM.NetId.Trim(), oldADGroupName,
                        _configuration["AppSettings:Domain"], _configuration["AppSettings:Container"], _configuration["AppSettings:Environment"]);

                }
                //if user not in new AD group, add
                if (!ActiveDirectoryService.IsUserGroupMember(userVM.NetId, newADGroupName, _configuration["AppSettings:Domain"], _configuration["AppSettings:Environment"]))
                {
                    error += ActiveDirectoryService.AddMemberToADGroup(userVM.NetId, newADGroupName,
                        _configuration["AppSettings:Domain"], _configuration["AppSettings:Container"], _configuration["AppSettings:Environment"]);
                }

                //remove all subroles and add them back if there are any
                var existingSubRoles = _userSubRoleRepository.Find(s => s.UserId == userVM.Id);
                if (existingSubRoles.Any())
                    _userSubRoleRepository.RemoveRange(existingSubRoles);


                //update SubRoles
                if (userVM.HasSubRole)
                {
                    
                    foreach (var subRole in userVM.AssignedSubRoles)
                    {
                        var subRoleId = _subRoleRepository
                            .Find(s => s.EtiologyId == subRole.Id && s.RoleId == userVM.RoleId)
                            .FirstOrDefault()!.Id;
                        var newAssignedSubRole = new UserSubRole();
                        newAssignedSubRole.UserId = userVM.Id;
                        newAssignedSubRole.SubRoleId = subRoleId;
                        _userSubRoleRepository.Insert(newAssignedSubRole);
                    }
                }

                existingUser.Active = "Yes";
                _userRepository.Update(existingUser);

                if (!string.IsNullOrEmpty(error))
                    return Json(new { Success = false, Message = error });
                return Json(new { Success = true, Message = new[] { new { id = userVM.Id, netId = userVM.NetId, message = "Information for " + userName + " is updated." } } });

            }
            else //add new person
            {

                //check for existing person, this should not happen but just in case
                //userVM.NetID = "";
                var existingUser = _userRepository.Find(p => p.NetId.Trim() == userVM.NetId.Trim()).FirstOrDefault();
                if (existingUser != null)
                    return Json(new { Success = false, Message = "This person is already in the Speech Accessibility application. Please try to Edit instead." });
                
                var newPerson = new User
                {
                    NetId = userVM.NetId.Trim(),
                    LastName = userVM.LastName,
                    FirstName = userVM.FirstName,
                    RoleId = userVM.RoleId,
                    Active = "Yes",
                    UpdateTS = DateTime.Now,
                    UpdateBy = User.Identity.Name
                };
                _userRepository.Insert(newPerson);

                if (userVM.HasSubRole)
                {
                    foreach (var subRole in userVM.AssignedSubRoles)
                    {
                        var subRoleId = _subRoleRepository
                            .Find(s => s.EtiologyId == subRole.Id && s.RoleId == userVM.RoleId)
                            .FirstOrDefault()!.Id;

                        var newAssignedSubRole = new UserSubRole
                        {
                            UserId = newPerson.Id,
                            SubRoleId = subRoleId
                        };
                        _userSubRoleRepository.Insert(newAssignedSubRole);
                    }
                }

                var role = _roleRepository.Find(r => r.Id == newPerson.RoleId).FirstOrDefault();
                var error = ActiveDirectoryService.AddMemberToADGroup(userVM.NetId, role.ADGroupName,
                    _configuration["AppSettings:Domain"], _configuration["AppSettings:Container"], _configuration["AppSettings:Environment"]);
                
                
                if (!string.IsNullOrEmpty(error))
                    return Json(new { Success = false, Message = error });
         
                return Json(new { Success = true, Message = new[] { new { id =userVM.Id, netId = userVM.NetId, message = userName + " is added to the Speech Accessibility application." } } });
            }
        }

        [HttpPost]
        public ActionResult DeleteUser(string netId)
        {
            //personId = 100;
            if (string.IsNullOrEmpty(netId))
                return Json(new { Success = false, Message = "None user is selected." });
            var user = _userRepository.Find(p => p.NetId == netId ).Include(r => r.Role).FirstOrDefault();

            var error = "";
            if (user == null)
            {
                //remove from AD group if there is any
                var roles = _roleRepository.Find(r => r.InUsed == "Yes" && r.Id != 1).ToList(); //can't remove from System Admin group
                var existingADName =
                    ActiveDirectoryService.GetMemberADGroupName(netId, roles,
                        _configuration["AppSettings:Domain"], _configuration["AppSettings:Environment"]);
                if (!string.IsNullOrEmpty(existingADName))
                {
                    error = ActiveDirectoryService.RemoveMemberFromADGroup(netId, existingADName, _configuration["AppSettings:Domain"], _configuration["AppSettings:Container"], _configuration["AppSettings:Environment"]);
                    if (!string.IsNullOrEmpty(error))
                        return Json(new { Success = false, Message = error });
                    return Json(new { Success = true, Message = netId + " is removed from Speech Accessibility application." });
                }

                return Json(new { Success = false, Message = "User is not found." });
            }
          
            //remove from AD group
            //var netID =user.NetId;
            error = ActiveDirectoryService.RemoveMemberFromADGroup(user.NetId, user.Role.ADGroupName,
                _configuration["AppSettings:Domain"], _configuration["AppSettings:Container"], _configuration["AppSettings:Environment"]);

            
            if (!string.IsNullOrEmpty(error))
                return Json(new { Success = false, Message = error });

            user.Active = "No";
            user.UpdateBy = User.Identity.Name;
            user.UpdateTS = DateTime.Now;
            _userRepository.Update(user);
            return Json(new { Success = true, Message = user.LastName + ", " + user.FirstName + " is removed from Speech Accessibility application." });

        }

        public IActionResult UpdateGiftCardAmount()
        {

            return View();
        }

        public ActionResult LoadEtiologyGiftCardsAmount()
        {

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var giftCards = _giftCardAmountRepository.GetAll();
            foreach (var giftCard in giftCards)
            {
                giftCard.EtiologyName = _etiologyRepository.Find(e => e.Id == giftCard.EtiologyId).FirstOrDefault()!.Name;
                var temp = _categoryRepository.Find(p => p.Id == giftCard.PromptCategoryId).FirstOrDefault();
                if(temp != null)
                    giftCard.PromptCategoryName = temp.Description;

            }

            var recordsTotal = giftCards.Count();
            giftCards = DynamicSortingExtensions<GiftCardAmount>.SetOrderByDynamic(giftCards, Request.Form);
            giftCards = giftCards.Skip(skip).Take(pageSize);
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = giftCards });

        }

        public IActionResult ContributorsPaidByCheckAndHelperNotPaid()
        {

            return View();
        }

        public ActionResult LoadContributorsPaidByCheck()
        {

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();

            var contributors = _contributorsPaidByCheckRepository.GetAll().OrderBy(c=>c.EmailDomain);
            var recordsTotal = contributors.Count();
            
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = contributors });
        }

        public ActionResult LoadHelpersShouldNotPaid()
        {

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
          

            var helpers = _helperNotPaidGiftCardsRepository.GetAll().OrderBy(c => c.LastName);
            var recordsTotal = helpers.Count();
            
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = helpers });

        }


        [HttpPost]
        public ActionResult UpdateContributorPaidByCheckDomain(int id, string domain)
        {
            if (id < 0)
                return Json(new { Success = false, Message = "No record is selected." });
            if (id == 0) //this is a new domain
            {
                if(domain=="")
                    return Json(new { Success = false, Message = "Domain cannot be empty." });

                var newDomain = new ContributorsPaidByCheck
                {
                    EmailDomain = domain
                };
                _contributorsPaidByCheckRepository.Insert(newDomain);
                return Json(new { Success = true, Message = "New domain is added." });
            }

            var emailDomain = _contributorsPaidByCheckRepository.Find(e => e.Id == id).FirstOrDefault();

            if (emailDomain == null || domain==null )
            {
                return Json(new { Success = false, Message = "Domain cannot be empty." });
            }

            emailDomain.EmailDomain = domain;
            _contributorsPaidByCheckRepository.Update(emailDomain);
            return Json(new { Success = true, Message = "Email Domain is updated." });

        }

        [HttpPost]
        public ActionResult UpdateHelperInformation(int id, string firstName, string lastName, string email)
        {
            if (id < 0)
                return Json(new { Success = false, Message = "No record is selected." });
            if (id == 0) //this is a new domain
            {
                if (firstName == "" || lastName=="" || email=="")
                    return Json(new { Success = false, Message = "FirstName, LastName and Email Address cannot be empty." });

                var newHelper = new HelperNotPaidGiftCards()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    HelperEmailAddress = email
                };
                _helperNotPaidGiftCardsRepository.Insert(newHelper);
                return Json(new { Success = true, Message = "New helper is added." });
            }

            var helper = _helperNotPaidGiftCardsRepository.Find(e => e.Id == id).FirstOrDefault();

            if (helper == null || firstName == "" || lastName == "" || email == "")
            {
                return Json(new { Success = false, Message = "FirstName, LastName and Email Address cannot be empty." });
            }

            helper.FirstName = firstName;
            helper.LastName = lastName;
            helper.HelperEmailAddress = email;
            _helperNotPaidGiftCardsRepository.Update(helper);
            return Json(new { Success = true, Message = "Helper information is updated." });

        }


        [HttpPost]
        public ActionResult DeleteContributorPaidByCheckDomain(int id)
        {
            if (id < 0)
                return Json(new { Success = false, Message = "No record is selected." });
            
            var emailDomain = _contributorsPaidByCheckRepository.Find(e => e.Id == id).FirstOrDefault();
            _contributorsPaidByCheckRepository.Delete(emailDomain);
            return Json(new { Success = true, Message = "Email Domain is deleted." });
        }


        [HttpPost]
        public ActionResult DeleteHelperInformation(int id)
        {
            if (id < 0)
                return Json(new { Success = false, Message = "No record is selected." });

            var helper = _helperNotPaidGiftCardsRepository.Find(e => e.Id == id).FirstOrDefault();
            _helperNotPaidGiftCardsRepository.Delete(helper);
            return Json(new { Success = true, Message = "Helper is deleted." });
        }

        [HttpPost]
        public ActionResult UpdateGiftCardAmounts(int id, int firstGiftCard, int secondGiftCard, int thirdGiftCard)
        {
            //personId = 100;
            if (id <= 0)
                return Json(new { Success = false, Message = "No record is selected." });
            var giftCard = _giftCardAmountRepository.Find(e => e.Id == id).FirstOrDefault();

           
            if (giftCard == null)
            {
                
                return Json(new { Success = false, Message = "Gift Card is not found." });
            }

            giftCard.FirstGiftCard = firstGiftCard;
            giftCard.SecondGiftCard= secondGiftCard;
            giftCard.ThirdGiftCard=thirdGiftCard;
            _giftCardAmountRepository.Update(giftCard);
            return Json(new { Success = true, Message = "Amount is updated." });

        }

        public IActionResult ContributorsSearch()
        {

            return View();
        }

        public ActionResult LoadContributors(string searchValue)
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            //var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            if (!string.IsNullOrEmpty(searchValue))
            {
                var contributors = _contributorViewRepository.Find(c => c.Id.ToString().Contains(searchValue)
                                                                        || c.FirstName.Contains(searchValue) || c.LastName.Contains(searchValue)
                                                                        || c.EmailAddress.Contains(searchValue));

                var recordsTotal = contributors.Count();
                contributors = DynamicSortingExtensions<ContributorView>.SetOrderByDynamic(contributors, Request.Form);
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = contributors });
            }
            else
            {
                var contributors = new ContributorView();
                return Json(new { draw = draw, recordsFiltered = 0, recordsTotal = 0, data = contributors });
            }

           
        }
        [HttpPost]
        public ActionResult EditContributorInfo(Guid contributorId, string contributorEmail, string helperInd, string helperEmail, string birthYear, string comments, string helperPhone)
        {
            //contributor email cannot be empty
            if (string.IsNullOrEmpty(contributorEmail))
            {
                return Json(new { Success = false, Message = "Contributor email cannot be empty." });
            }

            var contributor = _contributorRepository.Find(c => c.Id == contributorId).Include(c => c.IdentityUser).FirstOrDefault();
            if (contributor == null)
            {
                return Json(new { Success = false, Message = "Contributor is not found." });
            }

            if (contributor.EmailAddress != null && !contributor.EmailAddress.Equals(contributorEmail))
            {
                contributor.EmailAddress = contributorEmail;
                contributor.IdentityUser.UserName = contributorEmail;
                contributor.IdentityUser.NormalizedEmail = contributorEmail;
                contributor.IdentityUser.Email = contributorEmail;
                contributor.IdentityUser.NormalizedUserName = contributorEmail;
            }
            else
            {
                contributor.EmailAddress = contributorEmail;
                contributor.IdentityUser.UserName = contributorEmail;
                contributor.IdentityUser.NormalizedEmail = contributorEmail;
                contributor.IdentityUser.Email = contributorEmail;
                contributor.IdentityUser.NormalizedUserName = contributorEmail;
            }

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
            if (!string.IsNullOrEmpty(comments))
                contributor.Comments = comments;
            contributor.UpdateTS = DateTime.Now;
            contributor.ApproveDenyBy = User.Identity.Name;
            _contributorRepository.Update(contributor);

            return Json(new { Success = true, Message = "updated" });

        }


    }
}
