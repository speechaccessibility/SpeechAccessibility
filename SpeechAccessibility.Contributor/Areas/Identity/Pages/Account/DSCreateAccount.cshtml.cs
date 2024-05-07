using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpeechAccessibility.Data;
using SpeechAccessibility.Data.Entities;
using SpeechAccessibility.Models;
using SpeechAccessibility.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechAccessibility.Areas.Identity.Pages.Account
{
    public class DSCreateAccountModel : PageModel
    {      
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser>_userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IMailService _emailSender;
        private readonly IdentityContext _context;
        private readonly IConfiguration _config;
     
        public DSCreateAccountModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ILogger<RegisterModel> logger, IMailService emailSender, IdentityContext context, IConfiguration config)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _config = config;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public Guid ContributorId { get; set; }

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

        public List<String> resultErrorList = new List<String>();

        public string ReturnUrl { get; set; }

        public class InputModel
            {

                [Required]
                public string HelperInd { get; set; }

                [Required]
                [Display(Name = "First Name")]
                public string FirstName { get; set; }

                [Required]
                [Display(Name = "Last Name")]
                public string LastName { get; set; }

                [Required]
                [EmailAddress]
                [Display(Name = "Email")]
                public string Email { get; set; }
             
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
                [MinLength(10, ErrorMessage = "Phone number must be 10 digits")]
                [MaxLength(10)]
                [RegularExpression("^[0-9]*$", ErrorMessage = "Phone number must be numeric")]
                [Display(Name = "Phone Number")]
                public string PhoneNumber { get; set; }

                [Required]
                [Display(Name = "Current Age")]
                public string CurrentAge { get; set; }

                [Required]
                [Display(Name = "Diagnosis Age")]
                public string DiagnosisAge { get; set; }

                [Required]
                [Display(Name = "State")]
                public string State { get; set; }

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

                [MinLength(10, ErrorMessage = "Legal Guardian Phone number must be 10 digits")]
                [MaxLength(10)]
                [RegularExpression("^[0-9]*$", ErrorMessage = "Legal Guardian Phone number must be numeric")]
                [Display(Name = "Legal Guardian Phone Number")]
                public string LegalGuardianPhoneNumber { get; set; }

                [EmailAddress]
                [Display(Name = "Helper Email")]
                public string HelperEmail { get; set; }
               
                [Display(Name = "Helper First Name")]
                public string HelperFirstName { get; set; }

                [Display(Name = "Helper Last Name")]
                public string HelperLastName { get; set; }

                [MinLength(10, ErrorMessage = "Helper Phone number must be 10 digits")]
                [MaxLength(10)]
                [RegularExpression("^[0-9]*$", ErrorMessage = "Helper Phone number must be numeric")]
                [Display(Name = "Helper Phone Number")]
                public string HelperPhoneNumber { get; set; }

                public int ContributorStatus { get; set; }

            }
        


        public void OnGet(Guid contributorId)
        {
            ContributorId = contributorId;  
            Contributor contributor = _context.Contributor.Where(c => c.Id == ContributorId).FirstOrDefault();

            Input = new InputModel();

            Input.ContributorStatus = contributor.StatusId;

            Input.FirstName= contributor.FirstName;   
            Input.LastName= contributor.LastName;
            Input.Email = contributor.EmailAddress;
            Input.PhoneNumber = contributor.PhoneNumber;
            Input.State = contributor.StateResidence;
            Input.CurrentAge = contributor.CurrentAge;
            Input.DiagnosisAge = contributor.DiagnosisAge;
            Input.HelperInd = contributor.HelperInd;
            Input.HelperFirstName = contributor.HelperFirstName;
            Input.HelperLastName = contributor.HelperLastName;
            Input.HelperPhoneNumber = contributor.HelperPhoneNumber;
            Input.HelperEmail = contributor.HelperEmail;
            int legalGuardianCount = _context.LegalGuardian.Where(l => l.ContributorId == ContributorId).Count();
            Input.LegalGuardianInd = "I am my own legal guardian";

            if(legalGuardianCount> 0)
            {
                LegalGuardian legalGuardian = _context.LegalGuardian.Where(l => l.ContributorId == contributorId).FirstOrDefault();
                Input.LegalGuardianInd = "Someone else is my legal guardian";
                Input.LegalGuardianFirstName = legalGuardian.FirstName;
                Input.LegalGuardianLastName = legalGuardian.LastName;
                Input.LegalGuardianPhoneNumber = legalGuardian.PhoneNumber;
                Input.LegalGuardianEmail = legalGuardian.Email;
            }
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {

            returnUrl = returnUrl ?? Url.Content("~/");

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

                if (ModelState.IsValid)
            {

                IdentityUser user = new IdentityUser();            
                user.UserName = Input.Email;
                user.Email = Input.Email;

                string identityId = _context.Contributor.Where(c => c.Id == ContributorId).Select(c => c.IdentityUser.Id).FirstOrDefault();

                var result = new IdentityResult();

                if (identityId==null)
                {
                   
                    result = await _userManager.CreateAsync(user, Input.Password);
                }


                if (result.Succeeded)
                {
                    Contributor contributor = PopulateContributor(user);

                    PopulateLegalGuardian();

                    _context.SaveChanges();

                    SendEnrollmentEmail(contributor.EmailAddress);

                    await _signInManager.SignInAsync(user, isPersistent: false);

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

        private void PopulateLegalGuardian()
        {
            int legalGuardianCount = _context.LegalGuardian.Where(l => l.ContributorId == ContributorId).Count();
            LegalGuardian legalGuardian = new LegalGuardian();

            if (Input.LegalGuardianInd == "I am my own legal guardian")
            {
                //Remove existing legal guardian if they change to their own legal guardian
                if (legalGuardianCount > 0)
                {
                    legalGuardian = _context.LegalGuardian.Where(l => l.ContributorId == ContributorId).First();
                    _context.LegalGuardian.Remove(legalGuardian);
                }
            }
            else
            {
                if (legalGuardianCount > 0)
                {
                    //update existing legal guardian
                    legalGuardian = _context.LegalGuardian.Where(l => l.ContributorId == ContributorId).First();
                    legalGuardian.FirstName = Input.LegalGuardianFirstName;
                    legalGuardian.LastName = Input.LegalGuardianLastName;
                    legalGuardian.PhoneNumber = Input.LegalGuardianPhoneNumber;
                    legalGuardian.Email = Input.LegalGuardianEmail;
                }
                else
                {
                    //Add new legal guardian if no existing one
                    legalGuardian.FirstName = Input.LegalGuardianFirstName;
                    legalGuardian.LastName = Input.LegalGuardianLastName;
                    legalGuardian.PhoneNumber = Input.LegalGuardianPhoneNumber;
                    legalGuardian.Email = Input.LegalGuardianEmail;
                    legalGuardian.ContributorId = ContributorId;
                    _context.LegalGuardian.Add(legalGuardian);
                }

            }
        }

        private Contributor PopulateContributor(IdentityUser user)
        {
            Contributor contributor = _context.Contributor.Where(c => c.Id == ContributorId).First();

            contributor.FirstName = Input.FirstName;
            contributor.LastName = Input.LastName;
            contributor.PhoneNumber = Input.PhoneNumber;
            contributor.EmailAddress = Input.Email;
            contributor.HelperInd = Input.HelperInd;
            contributor.HelperFirstName = Input.HelperFirstName;
            contributor.HelperLastName = Input.HelperLastName;
            contributor.HelperEmail = Input.HelperEmail;
            contributor.HelperPhoneNumber = Input.HelperPhoneNumber;
            contributor.StateResidence = Input.State;
            contributor.CurrentAge = Input.CurrentAge;
            contributor.DiagnosisAge = Input.DiagnosisAge;
            contributor.IdentityUser = user;
            contributor.StatusId = 1;

            return contributor;
        }
    }
}
