using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using SpeechAccessibility.Data;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using SpeechAccessibility.Models;
using SpeechAccessibility.Data.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechAccessibility.Areas.Identity.Pages.Account
{
    public class DSAssentModel : PageModel
    {
        private readonly IdentityContext _identityContext;
        private readonly IConfiguration _config;

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel{
            public int Id { get; set; }

            [Required]
            [Display(Name = "Assent")]
            public string CapableOfReadingInd { get; set; }

            [MaxLength(50)]
            [Required]
            public string Name { get; set; }

            [MaxLength(50)]
            [Required]
            [Display(Name = "Name of Person Obtaining Assent")]
            public string PersonObtainingAssent { get; set; }
            public Guid ContributorId { get; set; }
            public int Version { get; set; }
            public DateTime CreateTS { get; set; }
            public DateTime UpdateTS { get; set; }

        }

        public DSAssentModel(IdentityContext identityContext, IConfiguration config)
        {
            _identityContext = identityContext;
            _config = config;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if(ModelState.IsValid)
            {
                Guid contributorId = new Guid(TempData["contributorId"].ToString());
                Assent assent = new Assent
                {
                    Name = Input.Name,
                    CapableOfReadingInd = Input.CapableOfReadingInd,
                    Version = Int32.Parse(_config["DSAssentVersion"]),
                    PersonObtainingAssent = Input.PersonObtainingAssent,
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
