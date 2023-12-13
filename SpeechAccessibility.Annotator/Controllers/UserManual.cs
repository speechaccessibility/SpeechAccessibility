using Microsoft.AspNetCore.Mvc;
using System;

namespace SpeechAccessibility.Annotator.Controllers
{
    public class UserManual : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SpeechFileManual()
        {
            return View();
        }
        public IActionResult ContributorManual()
        {
            return View();
        }
        public IActionResult PromptsManual()
        {
            return View();
        }
        public IActionResult BlocksManual()
        {
            return View();
        }

        public IActionResult ReportManual()
        {
            return View();
        }

        public IActionResult ManageUsersManual()
        {
            return View();
        }           
    
    }
}
