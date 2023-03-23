using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SpeechAccessibility.Annotator.Services;
using SpeechAccessibility.Core.Interfaces;

namespace SpeechAccessibility.Annotator.Controllers
{
    [Authorize(Policy = "AnnotatorAdmin")]
    public class ExtensionController : Controller
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        public ExtensionController(IRoleRepository roleRepository, IConfiguration configuration, IUserRepository userRepository)
        {
            _roleRepository = roleRepository;
            _configuration = configuration;
            _userRepository = userRepository;
        }

        [HttpPost]
        public ActionResult GetUserInfoFromAD(string netId)
        {
            //check to see if user is already in the system
            var existingUser = _userRepository.Find(u => u.NetId.Equals(netId)).Include(u=>u.Role).FirstOrDefault();
            if (existingUser == null)
            {
                var user = ActiveDirectoryService.GetUserInfoFromAD(netId, _configuration["AppSettings:Domain"]);

                if (user != null && !string.IsNullOrEmpty(user.NetId))
                {
                    return Json(new { Success = true, Message = user });
                }
                return Json(new { Success = false, Message = "NetID is not valid." });
            }

            return Json(new { Success = true, Message = existingUser });



        }
    }
}
