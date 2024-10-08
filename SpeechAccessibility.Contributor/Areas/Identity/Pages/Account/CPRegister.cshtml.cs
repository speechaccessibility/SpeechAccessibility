﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpeechAccessibility.Data;
using SpeechAccessibility.Data.Entities;
using SpeechAccessibility.Models;
using SpeechAccessibility.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Speech.Recognition;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpeechAccessibility.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class CPRegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IMailService _emailSender;
        private readonly IdentityContext _context;
        private readonly IConfiguration _config;

        public CPRegisterModel(
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
            _emailSender = emailSender;
            _context = context;
            _config = config;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public List<string> ExistingEmailList { get; set; }

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

        public List<String> resultErrorList = new List<String>();

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            //[Required]
            //[Display(Name = "Parkinson's Disease Indicator")]
            //public string parkinsonsInd { get; set; }

            public string otherText { get; set; }

            [Display(Name ="EighteenOrOlder")]
            public string eighteenOrOlderInd { get; set; }

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

            [Required]
            [Display(Name ="Phone Number")]
            public string phoneNumber { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

             public string ConfirmEmail { get; set; }

            [Required]
            [Display(Name = "Helper Indicator")]
            public string HelperInd { get; set; }

            [EmailAddress]
            [Display(Name = "Helper's Email")]
            public string HelperEmail { get; set; }     

            [Display(Name ="Helper's First Name")]
            public string HelperFirstName { get; set; }

            [Display(Name = "Helper's Last Name")]
            public string HelperLastName { get; set; }

            [Display(Name = "Helper Phone Number")]
            public string HelperPhoneNumber { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 9)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public bool ContactLSVT { get; set; }

            [Display(Name ="Birth Year")]
            public string BirthYear { get; set; }

            public int etiologyId { get; set; }

            [Required]
            [Display(Name = "Legal Guardian Indicator")]
            public string LegalGuardianInd { get; set; }

            [Display(Name = "Legal Guardian First Name")]
            public string LegalGuardianFirstName { get; set; }

            [Display(Name = "Legal Guardian Last Name")]
            public string LegalGuardianLastName { get; set; }

            [EmailAddress]
            [Display(Name = "Legal Guardian Email")]
            public string LegalGuardianEmail { get; set; }

            [Display(Name = "Legal Guardian Phone Number")]
            public string LegalGuardianPhoneNumber { get; set; }

            [Required]
            [Display(Name = "Current Age")]
            public string CurrentAge { get; set; }

            [Display(Name = "Diagnosis Age")]
            public string DiagnosisAge { get; set; }

            public string DuplicateEmailInd { get; set; }

            [Display(Name = "Time Zone")]
            public string TimeZone { get; set; }

            [Required]
            [MaxLength(150)]
            [Display(Name = "Reference Source")]
            public string ReferenceSource { get; set; }

            public string Country { get; set; }

        }
        public List<SelectListItem> ageList { get; } = getAgeList();

        private static List<SelectListItem> getAgeList()
        {
            List<SelectListItem> ageList = new List<SelectListItem>();
            for (int i = 1; i <= 99; i++)
            {
                SelectListItem item = new SelectListItem { Value = i.ToString(), Text = i.ToString() };
                ageList.Add(item);
            }
            SelectListItem lastItem = new SelectListItem { Value = "100", Text = "100+" };
            ageList.Add(lastItem);
            return ageList;
        }

        public IActionResult OnGet(int etiology, string otherEtiologyDescription)
        {
            Input = new InputModel();
            Input.etiologyId = etiology;
            Input.otherText = otherEtiologyDescription;

            ExistingEmailList = _context.Contributor.Select(c => c.EmailAddress).ToList();

            if (etiology == 0)
            {
                return RedirectToPage("./DiagnosisRegister");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
         
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (Input.etiologyId == 3 && String.IsNullOrEmpty(Input.DiagnosisAge))
            {
                ModelState.AddModelError("diagnosisAgeValidation", "Diagnosis age is required.");
            }

            if (Input.etiologyId == 3 && String.IsNullOrEmpty(Input.TimeZone))
            {
                ModelState.AddModelError("timeZoneValidation", "Time zone is required.");
            }

            if ("Yes".Equals(Input.HelperInd))
                {
                if (String.IsNullOrEmpty(Input.HelperEmail))
                {
                    ModelState.AddModelError("helperEmailValidation", "Helper email is required.");
                }

                if (String.IsNullOrEmpty(Input.HelperFirstName))
                {
                    ModelState.AddModelError("helperFirstNameValidation", "Helper first name is required.");
                }
                if (String.IsNullOrEmpty(Input.HelperLastName))
                {
                    ModelState.AddModelError("helperLastNameValidation", "Helper last name is required.");
                }
                if (String.IsNullOrEmpty(Input.HelperPhoneNumber))
                {
                    ModelState.AddModelError("helperPhoneNumberValidation", "Helper phone number is required.");
                }
                
            }

            if ("Someone else is my legal guardian".Equals(Input.LegalGuardianInd))
            {
                if (String.IsNullOrEmpty(Input.LegalGuardianFirstName))
                {
                    ModelState.AddModelError("lgFirstNameValidation", "Legal guardian first name is required.");
                }
                if (String.IsNullOrEmpty(Input.LegalGuardianLastName))
                {
                    ModelState.AddModelError("lgLastNameValidation", "Legal guardian last name is required.");
                }
                if (String.IsNullOrEmpty(Input.LegalGuardianPhoneNumber))
                {
                    ModelState.AddModelError("lgPhoneValidation", "Legal guardian phone number is required.");
                }
                if (String.IsNullOrEmpty(Input.LegalGuardianEmail))
                {
                    ModelState.AddModelError("lgEmailValidation", "Legal guardian email is required.");
                }
            }

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

                if (!Input.Email.Equals(Input.ConfirmEmail, StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("confirmEmailValidation", "The email and confirmation email do not match.");
                    return Page();
                }
                int age = Int32.Parse(Input.CurrentAge);


                if (unqualifiedStates.Contains(Input.state) || age < 18)
                {
                    return RedirectToPage("./Unqualified");
                }
                IdentityUser user = new IdentityUser();
                user.UserName = Input.Email;
                user.Email = Input.Email;
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    string isActive = _context.Etiology.Where(e => e.Id == Input.etiologyId).Select(e => e.Active).First();

                    Contributor contributor = PopulateContributor(user, isActive);
                    _context.Contributor.Add(contributor);

                    _context.Etiology.Remove(contributor.Etiology);
                    _context.SaveChanges();

                    if ("Someone else is my legal guardian".Equals(Input.LegalGuardianInd) || "Someone else is their legal guardian".Equals(Input.LegalGuardianInd))
                    {
                        LegalGuardian legalGuardian = new LegalGuardian();
                        legalGuardian.FirstName = Input.LegalGuardianFirstName;
                        legalGuardian.LastName = Input.LegalGuardianLastName;
                        legalGuardian.PhoneNumber = Input.LegalGuardianPhoneNumber;
                        legalGuardian.Email = Input.LegalGuardianEmail;
                        legalGuardian.ContributorId = contributor.Id;
                        _context.LegalGuardian.Add(legalGuardian);

                        _context.SaveChanges();
                    }

                    _logger.LogInformation("User created a new account with password.");

                    SendEnrollmentEmail(user.Email);

                    await _signInManager.SignInAsync(user, isPersistent: false);


                    string message = "<div>Hello,</div><br/><div>A potential Speech Accessibility Project participant, " + Input.firstName + ", has requested an assessment. You may contact them at " + Input.Email + " or " + Input.phoneNumber + ".";

                    if ("Someone else is my legal guardian".Equals(Input.LegalGuardianInd) || "Someone else is their legal guardian".Equals(Input.LegalGuardianInd))
                    {
                        message += " Their legal guardian is " + Input.LegalGuardianFirstName + " and their email is " + Input.LegalGuardianEmail + ".";
                    }

                    message += "</div>";

                    if (Input.etiologyId == 5)
                    {
                        message += "<div><br/>Their etiology is Other: " + Input.otherText + ".";
                    }

                    if (Input.etiologyId == 3)
                    {
                        message += "<div><br/>Their time zone is: " + Input.TimeZone + ".";
                    }
                    message += "<div><br/> The Speech Accessibility Project Team<br/> University of Illinois Urbana - Champaign</div>";
                    string to = _config["CPEmail"];

                    string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                    if (_config["DeveloperMode"].Equals("Yes") || !"Production".Equals(environment))
                    {
                        to = _config["TestEmail"];
                        string testMessage = "<p><strong>This email was sent in testing mode.</strong></p>";
                        message = testMessage + message;
                    }

                    await _emailSender.SendEmailAsync(to, "Assessment Request", message);


                    return RedirectToAction("RecordPrompt");


                }
                foreach (var error in result.Errors)
                {
                    if (error.Code == "DuplicateUserName")
                    {
                        error.Description += " Please click the Login link above to log in to an existing account. If you’ve forgotten your password, you can click the 'Forgot your password' link on the Login page to reset it.";
                    }
                    ModelState.AddModelError(string.Empty, error.Description);
                    resultErrorList.Add(error.Description);

                }
            }
            else {
                ExistingEmailList = _context.Contributor.Select(c => c.EmailAddress).ToList();

            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private void SendEnrollmentEmail(string emailAddress)
        {

            string message = "<p>Thank you for your interest in the Speech Accessibility Project. A speech pathologist will review the information your provided and determine if you are eligible for our study. You will receive another email in about 7-10 days to let you know whether you can contribute speech recordings for this study.</p>" +
                "<p>Thank you for your time and we will be in touch soon!</p><p>The Speech Accessibility Project Team</p>";


            string subject = "Thank you for enrolling in the Speech Accessibility Project study!";

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (_config["DeveloperMode"].Equals("Yes") || !"Production".Equals(environment))
            {
                emailAddress = _config["TestEmail"];
            }

            _emailSender.SendEmailAsync(emailAddress, subject, message);
        }

        private Contributor PopulateContributor(IdentityUser user, string isActive)
        {
            Contributor contributor = new Contributor();
            contributor.HelperInd = Input.HelperInd;
            contributor.HelperEmail = Input.HelperEmail;
            contributor.HelperFirstName = Input.HelperFirstName;
            contributor.HelperLastName = Input.HelperLastName;
            contributor.HelperPhoneNumber = Input.HelperPhoneNumber;
            contributor.FirstName = Input.firstName;
            contributor.MiddleName = Input.middleName;
            contributor.LastName = Input.lastName;
            contributor.StateResidence = Input.state;
            contributor.Etiology = new Etiology { Id = Input.etiologyId };
            contributor.IdentityUser = user;
            contributor.EighteenOrOlderInd = "Yes";
            contributor.StatusId =  1;
            contributor.ContactLSVT = Input.ContactLSVT;
            contributor.EmailAddress = Input.Email;
            contributor.PhoneNumber = Input.phoneNumber;
            contributor.OtherEtiologyText = Input.otherText;
            contributor.BirthYear = Input.BirthYear;
            contributor.CurrentAge= Input.CurrentAge;
            contributor.DiagnosisAge = Input.DiagnosisAge;
            contributor.TimeZone = Input.TimeZone;
            contributor.ReferenceSource = Input.ReferenceSource;
            contributor.Country = Input.Country;

            return contributor;
        }

    }
}
