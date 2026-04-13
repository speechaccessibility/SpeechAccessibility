using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpeechAccessibility.Data;
using SpeechAccessibility.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using System.Threading.Tasks;
using System.Linq;
using SpeechAccessibility.Models;
using SpeechAccessibility.Data.Entities;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SpeechAccessibility.Areas.Identity.Pages.Account
{
    public class StutteringRegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
     
        private readonly IdentityContext _context;
        private readonly IConfiguration _config;

        public StutteringRegisterModel(
             UserManager<IdentityUser> userManager,
             SignInManager<IdentityUser> signInManager,
             ILogger<RegisterModel> logger,
             IMailService emailSender,
             IdentityContext context,
             IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
            _config = config;
        }

        [BindProperty]
        public List<String> ExistingEmailList { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public List<String> unqualifiedStates = new List<String>
        {
            "IL",
            "TX",
            "WA",
            "None"
        };

        public List<SelectListItem> countryList { get; } = new List<SelectListItem>
        {
               new SelectListItem { Value = "United States", Text = "United States" },
                new SelectListItem { Value = "Canada", Text = "Canada" },
        };

        public List<SelectListItem> ratingList { get; } = new List<SelectListItem>
        { new SelectListItem{Value="1", Text="1" },
        new SelectListItem{Value="2", Text="2" },
        new SelectListItem{Value="3", Text="3" },
        new SelectListItem{Value="4", Text="4" },
        new SelectListItem{Value="5", Text="5" }
        };

        public List<SelectListItem> stateList { get; } = new List<SelectListItem>
         {
                    new SelectListItem { Value = "AL", Text = "Alabama" },
                    new SelectListItem { Value = "AK", Text = "Alaska" },
                    new SelectListItem { Value = "AZ", Text = "Arizona" },
                    new SelectListItem { Value = "AR", Text = "Arkansas" },
                    new SelectListItem { Value = "CA", Text = "California" },
                    new SelectListItem { Value = "CO", Text = "Colorado" },
                    new SelectListItem { Value = "CT", Text = "Connecticut" },
                    new SelectListItem { Value = "DE", Text = "Delaware" },
                    new SelectListItem { Value = "FL", Text = "Florida" },
                    new SelectListItem { Value = "GA", Text = "Georgia" },
                    new SelectListItem { Value = "HI", Text = "Hawaii" },
                    new SelectListItem { Value = "ID", Text = "Idaho" },
                    new SelectListItem { Value = "IL", Text = "Illinois" },
                    new SelectListItem { Value = "IN", Text = "Indiana" },
                    new SelectListItem { Value = "IA", Text = "Iowa" },
                    new SelectListItem { Value = "KS", Text = "Kansas" },
                    new SelectListItem { Value = "KY", Text = "Kentucky" },
                    new SelectListItem { Value = "LA", Text = "Louisiana" },
                    new SelectListItem { Value = "ME", Text = "Maine" },
                    new SelectListItem { Value = "MD", Text = "Maryland" },
                    new SelectListItem { Value = "MA", Text = "Massachusetts" },
                    new SelectListItem { Value = "MI", Text = "Michigan" },
                    new SelectListItem { Value = "MN", Text = "Minnesota" },
                    new SelectListItem { Value = "MS", Text = "Mississippi" },
                    new SelectListItem { Value = "MO", Text = "Missouri" },
                    new SelectListItem { Value = "MT", Text = "Montana" },
                    new SelectListItem { Value = "NC", Text = "North Carolina" },
                    new SelectListItem { Value = "ND", Text = "North Dakota" },
                    new SelectListItem { Value = "NE", Text = "Nebraska" },
                    new SelectListItem { Value = "NV", Text = "Nevada" },
                    new SelectListItem { Value = "NH", Text = "New Hampshire" },
                    new SelectListItem { Value = "NJ", Text = "New Jersey" },
                    new SelectListItem { Value = "NM", Text = "New Mexico" },
                    new SelectListItem { Value = "NY", Text = "New York" },
                    new SelectListItem { Value = "OH", Text = "Ohio" },
                    new SelectListItem { Value = "OK", Text = "Oklahoma" },
                    new SelectListItem { Value = "OR", Text = "Oregon" },
                    new SelectListItem { Value = "PA", Text = "Pennsylvania" },
                    new SelectListItem { Value = "PR", Text = "Puerto Rico" },
                    new SelectListItem { Value = "RI", Text = "Rhode Island" },
                    new SelectListItem { Value = "SC", Text = "South Carolina" },
                    new SelectListItem { Value = "SD", Text = "South Dakota" },
                    new SelectListItem { Value = "TN", Text = "Tennessee" },
                    new SelectListItem { Value = "TX", Text = "Texas" },
                    new SelectListItem { Value = "UT", Text = "Utah" },
                    new SelectListItem { Value = "VT", Text = "Vermont" },
                    new SelectListItem { Value = "VA", Text = "Virginia" },
                    new SelectListItem { Value = "WA", Text = "Washington" },
                    new SelectListItem { Value = "DC", Text = "Washington DC" },
                    new SelectListItem { Value = "WV", Text = "West Virginia" },
                    new SelectListItem { Value = "WI", Text = "Wisconsin" },
                    new SelectListItem { Value = "WY", Text = "Wyoming" },
                    new SelectListItem {Value ="None", Text="None of the above"}
                };

        public List<SelectListItem> yearList { get; } = getYearList();

        public List<StutterSituations> stutterSituations = new List<StutterSituations>();
        private static List<SelectListItem> getYearList()
        {
            List<SelectListItem> yearList = new List<SelectListItem>();
            for (int i = DateTime.Now.Year; i >= 1900; i--)
            {
                SelectListItem item = new SelectListItem { Value = i.ToString(), Text = i.ToString() };
                yearList.Add(item);
            }
            return yearList;
        }

        private static List<SituationRating> initializeSituationRatings(List<StutterSituations> stutterSituations)
        { 
            List<SituationRating> situationRatings = new List<SituationRating>();

            foreach (var item in stutterSituations)
            { 
                SituationRating rating = new SituationRating();
                rating.StutterSituationId = item.Id;
                situationRatings.Add(rating);
            }

            return situationRatings;
        }



        public List<String> resultErrorList = new List<String>();

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            //[Required]
            //[Display(Name = "Parkinson's Disease Indicator")]
            //public string parkinsonsInd { get; set; }

            public string otherText { get; set; }


            [Required]
            [Display(Name = "EighteenOrOlderInd")]
            public string eighteenOrOlderInd{ get; set; }

            [Display(Name = "State")]
            public string state { get; set; }

            [Required]
            [Display(Name = "First Name")]
            public string firstName { get; set; }

            [Display(Name = "Middle Name")]
            public string middleName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string lastName { get; set; }

       
            [MaxLength(10)]
            [RegularExpression("^[0-9]*$", ErrorMessage = "Phone number must be numeric")]
            [Display(Name = "Phone Number")]
            public string phoneNumber { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            public string ConfirmEmail { get; set; }


            [Display(Name = "Birth Year")]
            public string BirthYear { get; set; }

            public int etiologyId { get; set; }

            [MaxLength(150)]
            [Display(Name = "Reference Source")]
            public string ReferenceSource { get; set; }

            public string Country { get; set; }

            public string DuplicateEmailInd { get; set; }
       
            public StutteringScreening StutterScreening { get; set; }

            public List<SituationRating> SituationRating { get; set; }
        
        }
            public IActionResult OnGet(int etiology)
            {
                Input = new InputModel();
                Input.etiologyId = etiology;

            if (etiology == 0)
            {
                return RedirectToPage("./DiagnosisRegister");
            }
            
            ExistingEmailList = _context.Contributor.Select(c => c.EmailAddress).ToList();
            stutterSituations = _context.StutterSituations.ToList();
            return Page();
            }
            public async Task<IActionResult> OnPost()
            {
        
            if ("Yes".Equals(Input.DuplicateEmailInd))
            {
                ModelState.AddModelError("duplicateEmailValidation", "This email is already registered.");
            }
            if (ModelState.IsValid)
            {
                if ("United States".Equals(Input.Country))
                {

                    if (String.IsNullOrEmpty(Input.state))
                    {
                        ModelState.AddModelError("stateError", "State is required.");
                        return Page();
                    }
                }

                if (unqualifiedStates.Contains(Input.state) || "No".Equals(Input.eighteenOrOlderInd))

                {
                    return RedirectToPage("./Unqualified");
                }

                TempData["Input"] = JsonConvert.SerializeObject(Input);

                return RedirectToPage("./StutterConsent");

              

            }
            else
            {

                ExistingEmailList = _context.Contributor.Select(c => c.EmailAddress).ToList();
            }

            return Page();
        }

      

       
    }  
}
