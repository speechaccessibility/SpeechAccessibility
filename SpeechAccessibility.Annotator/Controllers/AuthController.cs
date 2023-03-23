using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SpeechAccessibility.Annotator.Services;
using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Controllers
{

    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;

        public AuthController(IConfiguration configuration, IRoleRepository roleRepository, IUserRepository userRepository)
        {
            _configuration = configuration;
            _roleRepository = roleRepository;
            _userRepository = userRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> SignIn(string returnUrl = null)
        {
            //Log.Debug("Inside SignIn Method");
            IHeaderDictionary headersDictionary = HttpContext.Request.Headers;
            var loggedInUser = new User();
            if (_configuration["AppSettings:DeveloperMode"] == "Yes")
            {
                loggedInUser.NetId = _configuration["AppSettings:TestingUser"];

            }
            else
            {
                loggedInUser.NetId = headersDictionary["SM_USER"].ToString();
                loggedInUser.LastName = headersDictionary["LAST_NAME"].ToString();
                loggedInUser.FirstName = headersDictionary["FIRST_NAME"].ToString();
            }


            //get roles name from database
            var allRoles = _roleRepository.Find(r => r.InUsed == "Yes" && !string.IsNullOrEmpty(r.ADGroupName)).ToList();

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, loggedInUser.NetId),
                _configuration["AppSettings:DeveloperMode"] == "Yes"
                    ? new Claim("FullName", loggedInUser.NetId)
                    : new Claim("FullName", loggedInUser.FirstName + " " + loggedInUser.LastName)

            };
            bool inGroup = false;
            var existInDatabase = _userRepository.Find(u => u.NetId == loggedInUser.NetId && u.Active == "Yes").FirstOrDefault();
            if (existInDatabase != null)
            {
               
                foreach (var role in allRoles)
                {
                    //user has to be in the database and AD group
                    if (role.Id == existInDatabase.RoleId)
                    {
                        inGroup = ActiveDirectoryService.IsUserGroupMember(loggedInUser.NetId, role.ADGroupName, _configuration["AppSettings:Domain"]
                            , _configuration["AppSettings:Environment"]);
                        if (inGroup)
                        {  //Log.Debug($"User is in group {role.Description}");
                            userClaims.Add(new Claim(ClaimTypes.Role, role.Name));
                            break;
                        }
                    }
                }

            }

            if (inGroup == false)
                userClaims.Add(new Claim(ClaimTypes.Role, "None"));

            //Log.Debug($"User is in group {inGroup}");
            var userIdentity = new ClaimsIdentity(userClaims, "User Identity");
            var userPrincipal = new ClaimsPrincipal(new[] { userIdentity });
            await HttpContext.SignInAsync("SpeechAccessibilityAuthentication", userPrincipal);

            return Redirect(returnUrl);
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            if (_configuration["AppSettings:DeveloperMode"] == "Yes")
            {
                return RedirectToAction(nameof(HomeController.Index), "Auth");

            }
            return Redirect(_configuration["AppSettings:LogOutLink"]);

        }

        [Authorize(Policy = "SystemAdmin")]
        public ActionResult ADGroups()
        {
            return View();
        }


        [Authorize(Policy = "SystemAdmin")]
        [HttpPost]
        public ActionResult ImportSystemAdmin()
        {
            using (var context = new PrincipalContext(ContextType.Domain, _configuration["AppSettings:Domain"]))
            {

                GroupPrincipal group = GroupPrincipal.FindByIdentity(context, IdentityType.Name, "Beckman ITS FT");
                if (group != null)
                {
                    foreach (Principal member in group.Members)
                    {
                        var principal = UserPrincipal.FindByIdentity(context, member.SamAccountName);

                        if (principal != null)
                        {
                            var user = _userRepository.Find(u => u.NetId == member.SamAccountName).FirstOrDefault();

                            if (user != null) continue;
                            user = new User
                            {
                                RoleId = 1,
                                LastName = principal.Surname,
                                FirstName = principal.GivenName,
                                NetId = principal.SamAccountName,
                                Active = "Yes",
                                UpdateTS = DateTime.Now,
                                UpdateBy = User.Identity.Name
                            };
                            _userRepository.Insert(user);
                        }
                    }
                }
            }

            return Json(new { Success = true, Message = "All System Admin are imported" });

        }




    }
}
