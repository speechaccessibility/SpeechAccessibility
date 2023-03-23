using System;
using System.Linq;
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
    [Authorize(Policy = "AnnotatorAdmin")]
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IRoleRepository _roleRepository;

        public AdminController(IUserRepository userRepository, IRoleRepository roleRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _configuration = configuration;
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
                    Value = a.Id.ToString(),
                    Text = a.Name
                })
                    .ToList(),


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

            var users = _userRepository.Find(u => u.Active == "Yes").Include(u => u.Role).Where(r => r.Role.Name.Contains(searchValue) || r.FirstName.Contains(searchValue) || r.LastName.Contains(searchValue) || r.NetId.Contains(searchValue));
            var recordsTotal = users.Count();

            var personsList = users.Skip(skip).Take(pageSize).Select(p => new ADMemberViewModel()
            {
                Id = p.Id,
                RoleId = p.RoleId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                NetId = p.NetId,
                RoleName = p.Role.Name
            });
            personsList = DynamicSortingExtensions<ADMemberViewModel>.SetOrderByDynamic(personsList, Request.Form);

            return Json(new { draw, recordsFiltered = recordsTotal, recordsTotal, data = personsList });

        }

        [HttpPost]
        public ActionResult AddUpdateUser(ADMemberViewModel userVM)
        {
            if (userVM.NetId == "")
            {
                return Json(new { Success = false, Message = "NetID is required." });
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
                    error = ActiveDirectoryService.RemoveMemberFromADGroup(userVM.NetId, oldADGroupName,
                        _configuration["AppSettings:Domain"], _configuration["AppSettings:Container"], _configuration["AppSettings:Environment"]);

                }
                //if user not in new AD group, add
                if (!ActiveDirectoryService.IsUserGroupMember(userVM.NetId, newADGroupName, _configuration["AppSettings:Domain"], _configuration["AppSettings:Environment"]))
                {
                    error += ActiveDirectoryService.AddMemberToADGroup(userVM.NetId, newADGroupName,
                        _configuration["AppSettings:Domain"], _configuration["AppSettings:Container"], _configuration["AppSettings:Environment"]);
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
                //userVM.NetID = "speyton";
                var existingUser = _userRepository.Find(p => p.NetId == userVM.NetId).FirstOrDefault();
                if (existingUser != null)
                    return Json(new { Success = false, Message = "This person is already in the Speech Accessibility application. Please try to Edit instead." });
                
                var newPerson = new User
                {
                    NetId = userVM.NetId,
                    LastName = userVM.LastName,
                    FirstName = userVM.FirstName,
                    RoleId = userVM.RoleId,
                    Active = "Yes",
                    UpdateTS = DateTime.Now,
                    UpdateBy = User.Identity.Name
                };
                _userRepository.Insert(newPerson);

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
          
            //todo: if this user has worked with the prompts, set in-active
            //var userRecording = _recordingRepository.Find(r=>r.)
           
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



    }
}
