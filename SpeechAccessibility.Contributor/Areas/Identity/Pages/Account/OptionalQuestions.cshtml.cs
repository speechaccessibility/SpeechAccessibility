using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System;
using static Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal.ExternalLoginModel;
using SpeechAccessibility.Data.Entities;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpeechAccessibility.Data;
using SpeechAccessibility.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace SpeechAccessibility.Areas.Identity.Pages.Account
{
    [Authorize]
    public class OptionalQuestionsModel : PageModel
    {

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<OptionalQuestionsModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IdentityContext _context;
        private readonly IConfiguration _config;

        public OptionalQuestionsModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender,
            IdentityContext context,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _context = context;
            _config = config;
        }


        [BindProperty]
        public InputModel Input { get; set; }

        public List<SelectListItem> stateList { get; } = new List<SelectListItem>
         {
                    new SelectListItem {Value="", Text = "Please Select" },
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
                    new SelectListItem { Value = "RI", Text = "Rhode Island" },
                    new SelectListItem { Value = "SC", Text = "South Carolina" },
                    new SelectListItem { Value = "SD", Text = "South Dakota" },
                    new SelectListItem { Value = "TN", Text = "Tennessee" },
                    new SelectListItem { Value = "TX", Text = "Texas" },
                    new SelectListItem { Value = "UT", Text = "Utah" },
                    new SelectListItem { Value = "VT", Text = "Vermont" },
                    new SelectListItem { Value = "VA", Text = "Virginia" },
                    new SelectListItem { Value = "WA", Text = "Washington" },
                    new SelectListItem { Value = "WV", Text = "West Virginia" },
                    new SelectListItem { Value = "WI", Text = "Wisconsin" },
                    new SelectListItem { Value = "WY", Text = "Wyoming" },
                    new SelectListItem {Value ="None", Text="None of the above"}
                };

        public List<SelectListItem> occupationList = new List<SelectListItem>
        {
         new SelectListItem {Value="", Text = "Please Select" },
         new SelectListItem { Value = "Management, business and financial", Text = "Management, business and financial" },
         new SelectListItem { Value = "Computer, engineering and science", Text = "Computer, engineering and science" },
         new SelectListItem { Value = "Education, legal, community service, arts, and media", Text = "Education, legal, community service, arts, and media" },
         new SelectListItem { Value = "Healthcare practitioners and technical occupations", Text = "Healthcare practitioners and technical occupations" },
         new SelectListItem { Value = "Protective service occupations (e.g., law enforcement, firefighting and prevention)", Text = "Protective service occupations (e.g., law enforcement, firefighting and prevention)" },
         new SelectListItem { Value = "Food preparation and serving", Text = "Food preparation and serving" },
         new SelectListItem { Value = "Sales and office occupations", Text = "Sales and office occupations" },
         new SelectListItem { Value = "Natural resources, construction, and maintenance occupations", Text = "Natural resources, construction, and maintenance occupations" },
         new SelectListItem { Value = "Production, transportation, and material moving occupations", Text = "Production, transportation, and material moving occupations" },
         new SelectListItem { Value = "Homemaker", Text = "Homemaker" },
         new SelectListItem { Value = "Student", Text = "Student" },
         new SelectListItem {Value="Prefer not to answer", Text="Prefer not to answer"}
        };

        public List<SelectListItem> educationList = new List<SelectListItem>
        {
            new SelectListItem {Value="", Text = "Please Select" },
            new SelectListItem {Value="Did not finish high school", Text = "Did not finish high school" },
            new SelectListItem {Value="High school diploma or equivalent", Text = "High school diploma or equivalent" },
            new SelectListItem {Value="Some college", Text = "Some college" },
            new SelectListItem {Value="College degree", Text = "College degree" },
            new SelectListItem {Value="Graduate or professional degree", Text = "Graduate or professional degree" },
            new SelectListItem {Value="Prefer not to answer", Text = "Prefer not to answer" },
        };

        public List<SelectListItem> ageList { get; } = getAgeList();
        public List<SelectListItem> otherLanguageAgeList { get; } = getOtherAgeList();

        private static List<SelectListItem> getAgeList()
        {
            List<SelectListItem> ageList = new List<SelectListItem>();
            SelectListItem pleaseSelect = new SelectListItem {Value="" ,Text = "Please select" };
            SelectListItem preferNotToAnswer = new SelectListItem { Value = "Prefer not to answer", Text = "Prefer not to answer" };
            ageList.Add(pleaseSelect);
            ageList.Add(preferNotToAnswer);
            for (int i = 18; i < 100; i++)
            {
                SelectListItem item = new SelectListItem { Value = i.ToString(), Text = i.ToString() };
                ageList.Add(item);
            }
            SelectListItem oneHundredPlus = new SelectListItem { Value = "100+", Text = "100+" };
            ageList.Add(oneHundredPlus);
            return ageList;
        }

        private static List<SelectListItem> getOtherAgeList()
        {
            List<SelectListItem> ageList = new List<SelectListItem>();
            SelectListItem pleaseSelect = new SelectListItem { Value = "", Text = "Please select" };
            SelectListItem preferNotToAnswer = new SelectListItem { Value = "Prefer not to answer", Text = "Prefer not to answer" };
            ageList.Add(pleaseSelect);
            ageList.Add(preferNotToAnswer);
            for (int i = 1; i < 101; i++)
            {
                SelectListItem item = new SelectListItem { Value = i.ToString(), Text = i.ToString() };
                ageList.Add(item);
            }
            return ageList;
        }


        public class InputModel
        {
            public bool americanIndianOrAlaskaNative { get; set; }
            public bool asian { get; set; }
            public bool blackOrAfricanAmerican { get; set; }
            public bool white { get; set; }
            public bool otherRace { get; set; }
            public string otherRaceText { get; set; }
            public bool preferNotToAnswerRace { get; set; }

            [Required]
            [Display(Name ="Hispanic or Latino")]
            public string hispanicOrLatino { get; set; }

            [Required]          
            [Display(Name = "Gender")]
            public string gender { get; set; }
            [Required]
            [MinLength(2, ErrorMessage = "You must select an age")]
            public string age { get; set; }
            [Required]
            [MinLength(2, ErrorMessage = "You must select a state")]
            public string state { get; set; }
            [Required]
            [Display(Name = "Converse in English")]
            public string converseInEnglish { get; set; }

            public string otherLanguage { get; set; }   
            public string converseInOtherLanguageAge { get; set; }

            [Required]
            [Display(Name = "familiar people at home rating")]
            public string familiarPeopleAtHomeRating { get; set; }
            [Required]
            [Display(Name = "unfamiliar people at home rating")]
            public string unfamiliarPeopleAtHomeRating { get; set; }

            [Display(Name = "familiar people on phone rating")]
            public string familiarPeopleOnPhoneRating { get; set; }

            [Display(Name = "unfamiliar people on phone rating")]
            public string unfamiliarPeopleOnPhoneRating { get; set; }
            [Required]
            [Display(Name = "noisy environment rating")]
            public string noisyEnvironmentRating { get; set; }
            [Display(Name = "travel in car rating")]
            public string travelInCarRating { get; set; }
            [Display(Name = "long conversation rating")]
            public string longConversationRating { get; set; }
            [Display(Name = "upset or angry rating")]
            public string upsetOrAngryRating { get; set; }
            [Display(Name = "frustrated by speech rating")]
            public string frustratedBySpeechRating { get; set; }
            [Required]
            [Display(Name = "rely on others rating")]
            public string relyOnOthersRating { get; set; }
            [Required]
            [Display(Name = "repeat myself rating")]
            public string repeatMyselfRating { get; set; }

            [Required]
            [Display(Name = "difficulty hearing rating")]
            public string difficultyHearingRating { get; set; }

            [Display(Name = "avoid speech when tired rating")]
            public string avoidSpeechWhenTiredRating { get; set; }

            [Display(Name = "impact social activities rating")]
            public string impactSocialActivitiesRating { get; set; }

            [Required]
            [Display(Name = "Speech change")]
            public string speechChange { get; set; }

            public string speechChangeDescription { get; set; }

            [Required]
            [Display(Name ="Occupation")]
            public string occupation { get; set; }

            [Required]
            [Display(Name ="Education")]
            public string education { get; set; }

            [Required]
            [Display(Name = "Retired")]
            public string retired { get; set; } 
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!Input.americanIndianOrAlaskaNative && !Input.asian && !Input.blackOrAfricanAmerican && !Input.white && !Input.otherRace && !Input.preferNotToAnswerRace)
            {
                ModelState.AddModelError("racialGroupValidation", "Racial group is a required field.");
            }

            if ("No".Equals(Input.converseInEnglish) && Input.converseInOtherLanguageAge==null)
            {
                ModelState.AddModelError("otherLanguageAgeValidation", "Please select an age.");
            }

            if (ModelState.IsValid)
            {
                Guid contributorId = new Guid(TempData["contributorId"].ToString());
                Contributor contributor = _context.Contributor.Where(c => c.Id == contributorId).FirstOrDefault();

                ContributorDetails details = PopulateContributorDetails(contributor);

                _context.Add(details);
                _context.SaveChanges();

                CheckSelectedRacialGroups(details);

                return RedirectToAction("RecordPrompt");
            }
            return Page();
        }

        private void CheckSelectedRacialGroups(ContributorDetails details)
        {
            if (Input.americanIndianOrAlaskaNative)
            {
                addContributorRace(details, 1, null);
            }
            if (Input.asian)
            {
                addContributorRace(details, 2, null);
            }
            if (Input.blackOrAfricanAmerican)
            {
                addContributorRace(details, 3, null);
            }
            if (Input.white)
            {
                addContributorRace(details, 4, null);
            }
            if (Input.otherRace)
            {
                addContributorRace(details, 5, Input.otherRaceText);

            }
            if (Input.preferNotToAnswerRace)
            {
                addContributorRace(details, 6, null);
            }
        }

        private ContributorDetails PopulateContributorDetails(Contributor contributor)
        {
            return new ContributorDetails
            {
                Contributor = contributor,
                HispanicOrLatino = Input.hispanicOrLatino,
                Gender = Input.gender,
                Age = Input.age,
                State = Input.state,
                ConverseInEnglish = Input.converseInEnglish,
                OtherLanguage = Input.otherLanguage,
                ConverseInOtherLanguageAge = Input.converseInOtherLanguageAge,
                FamiliarPeopleAtHomeRating = Input.familiarPeopleAtHomeRating,
                UnfamiliarPeopleAtHomeRating = Input.unfamiliarPeopleAtHomeRating,
                NoisyEnvironmentRating = Input.noisyEnvironmentRating,
                RelyOnOthersRating = Input.relyOnOthersRating,
                RepeatMyselfRating = Input.repeatMyselfRating,
                DifficultyHearingRating = Input.difficultyHearingRating,
                SpeechChange = Input.speechChange,
                Occupation = Input.occupation,
                Education = Input.education,
                Retired = Input.retired
            };
        }

        private void addContributorRace(ContributorDetails details, int racialGroupId, string otherText)
        {
            RacialGroup racialGroup = new RacialGroup
            {
                Id = racialGroupId
            };
            ContributorRace contributorRace = new ContributorRace
            {
                RacialGroup = racialGroup,
                ContributorDetails = details,
                OtherRace = otherText
            };

            _context.ContributorRace.Add(contributorRace);
            _context.RacialGroup.Remove(racialGroup);
            _context.SaveChanges();
        }
    }
}
