using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpeechAccessibility.Data;
using SpeechAccessibility.Services;
using SpeechAccessibility.Models;
using SpeechAccessibility.Data.Entities;
using System.Linq;


namespace SpeechAccessibility.Areas.Identity.Pages.Account
{
    public class AphasiaRegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IMailService _emailSender;
        private readonly IdentityContext _context;
        private readonly IConfiguration _config;

        public AphasiaRegisterModel(UserManager<IdentityUser> userManager,
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
        public InputModel Input {get;set;}

        [BindProperty]
        public string AphasiaInd { get; set; }

        [BindProperty]
        public List<String> ExistingEmailList { get; set; }


        public List<String> resultErrorList = new List<String>();

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

        public List<SelectListItem> ageList { get; } = getAgeList();

        private static List<SelectListItem> getAgeList()
        {
            List<SelectListItem> ageList = new List<SelectListItem>();          
            for (int i = 1; i <= 99; i++)
            {
                SelectListItem item = new SelectListItem { Value = i.ToString(), Text = i.ToString() };
                ageList.Add(item);
            }
            SelectListItem lastItem = new SelectListItem {Value = "100", Text="100+"};
            ageList.Add(lastItem);
            return ageList;
        }
        public class InputModel
        {
            public string etiologyId { get; set; }

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [EmailAddress]
            public string ConfirmEmail { get; set; }

            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }

            [Required]
            [Display(Name = "Current Age")]
            public string CurrentAge { get; set; }

            [Required]
            [Display(Name = "Length of Diagnosis")]
            public string LengthOfDiagnosis { get; set; }

       
            [Display(Name = "State")]
            public string State { get; set; }

            [EmailAddress]
            [Display(Name = "Helper Email")]
            public string HelperEmail { get; set; }

            [EmailAddress]
             public string ConfirmHelperEmail { get; set; }

            [Display(Name = "Helper First Name")]
            public string HelperFirstName { get; set; }

            [Display(Name = "Helper Last Name")]
            public string HelperLastName { get; set; }

            [Display(Name = "Helper Phone Number")]
            public string HelperPhoneNumber { get; set; }

            public string Correspondence { get; set; }

            public string DuplicateEmailInd { get; set; }

            [Required]
            [MaxLength(150)]
            [Display(Name = "Reference Source")]
            public string ReferenceSource { get; set; }

            public string Country { get; set; }
        }

        public IActionResult OnGet(string aphasiaInd)
        {
            Input = new InputModel();
            AphasiaInd = aphasiaInd;

            ExistingEmailList = _context.Contributor.Select(c => c.EmailAddress).ToList();

            if (aphasiaInd == null)
            {
                return RedirectToPage("./AphasiaPreRegister");
            }

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {

            if ("No".Equals(AphasiaInd))
            {
                if (String.IsNullOrEmpty(Input.Correspondence))
                {
                    ModelState.AddModelError("correspondenceValidation", "Correspondence field is required");
                }
                
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


            if ("Yes".Equals(AphasiaInd) || "Individual".Equals(Input.Correspondence))
            {
                if (String.IsNullOrEmpty(Input.Email))
                {
                    ModelState.AddModelError("EmailValidation", " Email required");
                }

                if (String.IsNullOrEmpty(Input.PhoneNumber))
                {
                    ModelState.AddModelError("PhoneValidation", "Phone number required");
                }
            }

            if (Input.Email != null && !Input.Email.Equals(Input.ConfirmEmail, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("compareEmailValidation", "The email and confirmation email do not match.");
            }

            if (Input.HelperEmail != null && !Input.HelperEmail.Equals(Input.ConfirmHelperEmail, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("compareHelperEmailValidation", "The helper email and confirmation email do not match.");
            }

            if ("Yes".Equals(Input.DuplicateEmailInd))
            {
                ModelState.AddModelError("duplicateEmailValidation", "This email is already registered.");
            }

            if (ModelState.IsValid)
            {
                if ("United States".Equals(Input.Country))
                {

                    if (String.IsNullOrEmpty(Input.State))
                    {
                        ModelState.AddModelError("stateError", "State is required.");
                        return Page();
                    }
                }

                int age = Int32.Parse(Input.CurrentAge);


                if (unqualifiedStates.Contains(Input.State) || age < 18)
                {
                    return RedirectToPage("./Unqualified");
                }


                Contributor contributor = PopulateContributor();
                _context.Contributor.Add(contributor);

                _context.Etiology.Remove(contributor.Etiology);

                _context.SaveChanges();

                _logger.LogInformation("User created a new account with password.");


                string email = Input.Email;
                string phone = Input.PhoneNumber;

                if (String.IsNullOrEmpty(email))
                {
                    email = Input.HelperEmail;
                }

                if (String.IsNullOrEmpty(phone))
                {
                    phone = Input.HelperPhoneNumber;
                }

                SendNotificationEmail(email, phone);

                return RedirectToAction("ApprovalRequired", new { etiologyId = 4 });

            }
            else {
                ExistingEmailList = _context.Contributor.Select(c => c.EmailAddress).ToList();

            }

            // If we got this far, something failed, redisplay form
            return Page();

        }

        private void SendNotificationEmail(string email, string phone)
        {
            string message = "<div>Hello,</div><br/><div>A potential Speech Accessibility Project participant, " + Input.FirstName + ", has requested an assessment. You may contact them at " + email + " or " + phone + "</div><div><br/>The Speech Accessibility Project Team<br/>University of Illinois Urbana-Champaign</div>";

            string to = _config["AphasiaEmail"];

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (_config["DeveloperMode"].Equals("Yes") || !"Production".Equals(environment))
            {
                to = _config["TestEmail"];
                string testMessage = "<p><strong>This email was sent in testing mode.</strong></p>";
                message = testMessage + message;
            }

            _emailSender.SendEmailAsync(to, "Assessment Request", message);
        }

        

        private Contributor PopulateContributor()
        {
            Contributor contributor = new Contributor();

            contributor.FirstName = Input.FirstName;
            contributor.LastName = Input.LastName;
            contributor.StateResidence = Input.State;
            contributor.Etiology = new Etiology { Id = 4 };
            contributor.StatusId = 5;          
            contributor.EighteenOrOlderInd = "Yes";
            contributor.CurrentAge = Input.CurrentAge;
            contributor.LengthOfDiagnosis = Input.LengthOfDiagnosis;
            contributor.ReferenceSource = Input.ReferenceSource;
            contributor.Country = Input.Country;
           
            if ("Yes".Equals(AphasiaInd))
            {
                contributor.EmailAddress = Input.Email;
                contributor.PhoneNumber = Input.PhoneNumber;
                contributor.HelperInd = "No";           
            }
            else {

                    contributor.HelperInd = "Yes";
                    contributor.HelperEmail = Input.HelperEmail;
                    contributor.HelperPhoneNumber = Input.HelperPhoneNumber;
                    contributor.HelperFirstName = Input.HelperFirstName;
                    contributor.HelperLastName = Input.HelperLastName;

                if (Input.Correspondence == "Self")
                {
                    //Make the contributor's contact info the same as the caregiver
                    contributor.PhoneNumber = Input.HelperPhoneNumber;
                    contributor.EmailAddress = Input.HelperEmail;
                }
                else
                {
                    contributor.PhoneNumber = Input.PhoneNumber;
                    contributor.EmailAddress = Input.Email;
                }
                
            }
            return contributor;
        }


    }


}
