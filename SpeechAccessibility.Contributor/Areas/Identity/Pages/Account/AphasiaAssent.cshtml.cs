using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using System;
using SpeechAccessibility.Data.Entities;
using SpeechAccessibility.Data;
using Microsoft.Extensions.Configuration;

namespace SpeechAccessibility.Areas.Identity.Pages.Account
{
    public class AphasiaAssentModel : PageModel
    {
        private readonly IdentityContext _identityContext;
        private readonly IConfiguration _config;

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public int Id { get; set; }

            [Required]
            [Display(Name = "Assent")]
            public string CapableOfReadingInd { get; set; }          
            public Guid ContributorId { get; set; }
            public int Version { get; set; }
            public DateTime CreateTS { get; set; }
            public DateTime UpdateTS { get; set; }

        }

        public AphasiaAssentModel(IdentityContext identityContext, IConfiguration config)
        {
            _identityContext = identityContext;
            _config = config;
        }
        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                Guid contributorId = new Guid(TempData["contributorId"].ToString());
                Assent assent = new Assent
                {
                    CapableOfReadingInd = Input.CapableOfReadingInd,
                    Version = Int32.Parse(_config["AphasiaAssentVersion"]),
                    ContributorId = contributorId
                };

                _identityContext.Assent.Add(assent);
                _identityContext.SaveChanges();

                return RedirectToAction("RecordPrompt");
            }
            return Page();
        }
    }
}
