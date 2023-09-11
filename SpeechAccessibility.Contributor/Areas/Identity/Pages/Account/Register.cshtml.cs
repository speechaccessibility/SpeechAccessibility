using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpeechAccessibility.Data;
using SpeechAccessibility.Models;
using SpeechAccessibility.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpeechAccessibility.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IMailService _emailSender;
        private readonly IdentityContext _context;
        private readonly IConfiguration _config;

        public RegisterModel(
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

        public List<String> unqualifiedStates = new List<String>
        {
            "IL",
            "TX",
            "WA",
            "None"
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
            [Required]
            [Display(Name = "Parkinson's Disease Indicator")]
            public string parkinsonsInd { get; set; }

            [Required]
            [Display(Name ="EighteenOrOlder")]
            public string eighteenOrOlderInd { get; set; }

            [Required]
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
            [MinLength(10, ErrorMessage="Phone number must be 10 digits")]
            [MaxLength(10)]
            [RegularExpression("^[0-9]*$", ErrorMessage = "Phone number must be numeric")]
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

            [MaxLength(10)]
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
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
         
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

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
                else
                {
                    bool result = Regex.Match(Input.HelperPhoneNumber, @"^[0-9]*$").Success;

                    if (!result)
                    {
                        ModelState.AddModelError("helperPhoneNumberValidation", "Helper phone number must be numeric.");
                    }
                }
                
            }

            if ("Yes".Equals(Input.eighteenOrOlderInd)) { 
            
                if(String.IsNullOrEmpty(Input.BirthYear)) {
                    ModelState.AddModelError("birthYearValidation", "Birth year is required.");
                }
            }

            if (ModelState.IsValid)
            {

                if (!Input.Email.Equals(Input.ConfirmEmail, StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("confirmEmailValidation", "The email and confirmation email do not match.");
                    return Page();
                }
                
                if (unqualifiedStates.Contains(Input.state) || "No".Equals(Input.eighteenOrOlderInd))
                {
                    return RedirectToPage("./Unqualified");
                }
                IdentityUser user = new IdentityUser();
                user.UserName = Input.Email;
                user.Email = Input.Email;
                var result = await _userManager.CreateAsync(user, Input.Password);               

                if (result.Succeeded)
                {
                    Contributor contributor = PopulateContributor(user);
                    _context.Contributor.Add(contributor);
                    _context.ContributorStatus.Remove(contributor.Status);
                    _context.SaveChanges();

                    _logger.LogInformation("User created a new account with password.");

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    SendEnrollmentEmail(user.Email);

                    if (Input.parkinsonsInd=="Yes")
                    {
                        string message = "<div>Hello,</div><br/><div>A potential Speech Accessibility Project participant, " + Input.firstName +  ", has requested an assessment. You may contact them at " + Input.Email + " or " + Input.phoneNumber + "</div><div><br/>The Speech Accessibility Project Team<br/>University of Illinois Urbana-Champaign</div>";

                        string to = _config["LSVTEmail"];

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
                    else
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }


                }
                foreach (var error in result.Errors)
                {   if ( error.Code== "DuplicateUserName")
                    {
                        error.Description += " Please click the Login link above to log in to an existing account. If you’ve forgotten your password, you can click the 'Forgot your password' link on the Login page to reset it.";
                    }
                    ModelState.AddModelError(string.Empty, error.Description);
                    resultErrorList.Add(error.Description);

                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private void SendEnrollmentEmail(string emailAddress)
        {
       
            string message = "<div>Thank you for enrolling in the Speech Accessibility Project study! </div><br/>" +
                "<div>Please find below answers to frequently asked questions about how you’ll be compensated. Please email <a href=mailto:speechaccessibility@beckman.illinois.edu>speechaccessibility@beckman.illinois.edu</a> if you have any questions.</div><br/>" +
                "<div>Thank You!</div><div>The Speech Accessibility Project Team</div><br/>" +
                "<div><strong>Will I be compensated and how does it work?</strong></div>" +
                "<div>Participants receive $60 gift codes, three times during their participation. Those completing the full project will receive a total of $180 in three increments, each occurring approximately every one-third of the way through the recordings. Payments are Amazon eCodes, sent to the email address you provided when you signed up. It may take a few days after completion for your gift card to arrive in your email. We often issue eCodes on Wednesday and Fridays.</div><br/>" +
                "<div>The beginning of the email will read:</div></br>" +
                "<div>Dear Participant, </div><br/>" +
                "<div>Thank you for participating in the Speech Accessibility Project! We really appreciate your help. Below is the information about your Amazon.com* claim code.</div><br/>" +
                "<div>Amount: $0.00</div><br/>" +
                "<div><strong>Claim code: xxxx-xxxxxx-xxxx</strong></div><br/>" +
                "<div><strong>I have a caregiver helping me through the project. How will my caregiver be compensated?</strong></div>" +
                "<div>When you sign up for the study, you have the option to enter an email address for your caregiver. That person will be compensated with up to $90 in Amazon eCodes in three increments, each occurring approximately every one-third of the way through the participant recordings.</div><br/>" +
                "<div><strong>What if I didn’t enter the name of my caregiver at the beginning?</strong></div>" +
                "<div>Let your mentor know and they can assist you. You can also email: <a href=mailto:speechaccessibility@beckman.illinois.edu>speechaccessibility@beckman.illinois.edu</a> Or, call 1-888-309-6499.</div><br/>" +
                "<div><strong>I just finished [1/3rd, 2/3rd or all] of my recordings. Where’s my eCode?</strong></div>" +
                "<div>Our payment coordinator sends eCodes twice a week: often on Wednesdays and Fridays. If you finish a block on Saturday, you likely won’t receive your eCode until the middle of the following week.</div><br/>" +
                "<div><strong>I never got my eCode. Why?</strong></div>" +
                "<div>Sometimes, eCodes go directly to your spam folder. Please check there first. It will come from the Speech Accessibility Project email at <a href=mailto:speechaccessibility@beckman.illinois.edu>speechaccessibility@beckman.illinois.edu</a>. Our payment coordinator does not send eCodes every day and never on weekends. If you finish a block on Saturday, you will not receive your eCode for a few days</div><br/>" +
                "<div><strong>Can I be compensated with anything other than Amazon eCodes?</strong></div>" +
                "<div>Unfortunately, our project cannot compensate you in any other form at this time.</div>";

            string subject = "Thank you for enrolling in the Speech Accessibility Project study!";

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (_config["DeveloperMode"].Equals("Yes") || !"Production".Equals(environment))
            {
                emailAddress = _config["TestEmail"];
            }

            _emailSender.SendEmailAsync(emailAddress, subject, message);
        }

        private Contributor PopulateContributor(IdentityUser user)
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
            contributor.ParkinsonsInd = Input.parkinsonsInd;
            contributor.IdentityUser = user;
            contributor.EighteenOrOlderInd = Input.eighteenOrOlderInd;
            contributor.Status = new ContributorStatus { Id = 1 };
            contributor.ContactLSVT = Input.ContactLSVT;
            contributor.EmailAddress = Input.Email;
            contributor.PhoneNumber = Input.phoneNumber;
            contributor.BirthYear = Input.BirthYear;
            return contributor;
        }

    }
}
