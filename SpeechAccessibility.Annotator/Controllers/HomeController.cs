using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpeechAccessibility.Annotator.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using SpeechAccessibility.Core.Interfaces;

namespace SpeechAccessibility.Annotator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEtiologyRepository _etiologyRepository;
        public HomeController(ILogger<HomeController> logger, IEtiologyRepository etiologyRepository)
        {
            _logger = logger;
            _etiologyRepository = etiologyRepository;
        }

        [Authorize(Policy = "AllAnnotator")]
        public IActionResult Index()
        {
            //ViewBag.SubRoles = CacheExtension.GetEtiologyList(_etiologyRepository);
            //_etiologyRepository.Find(e => e.Active == "Yes").OrderBy(r => r.DisplayOrder).ToList();
            //Session["myListString"]
            return View();
        }

        [Authorize(Policy = "AllAnnotator")]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            //return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            return View();
        }

       
    }
}
