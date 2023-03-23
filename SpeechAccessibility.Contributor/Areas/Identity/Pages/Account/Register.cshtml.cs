using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpeechAccessibility.Data;
using SpeechAccessibility.Data.Entities;
using SpeechAccessibility.Models;
using SpeechAccessibility.Services;

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

        public PersonalInformationModel.InputModel PersonalInformation { get; set; }

        public List<String> resultErrorList = new List<String>();

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            public string state { get; set; }
            public string understandSpeechInd { get; set; }
            public string parkinsonsInd { get; set; }
            public string eighteenOrOlderInd { get; set; }

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
        }

        public async Task OnGetAsync(PersonalInformationModel.InputModel personalInformation,string returnUrl = null)
        {
            // PersonalInformation = personalInformation;
            if (Input == null)
            {
                Input = new InputModel
                {
                    understandSpeechInd = personalInformation.understandSpeechInd,
                    parkinsonsInd = personalInformation.parkinsonsInd,
                    state = personalInformation.state,
                    eighteenOrOlderInd = personalInformation.eighteenOrOlderInd,
                    ContactLSVT = true
                };
            }
            
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
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
            }
            
            if (ModelState.IsValid)
            {
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
                    if (Input.ContactLSVT)
                    {
                        string message = "<div>Hello,</div><br/><div>A potential Speech Accessibility Project participant has requested an assessment. You may contact them at " + Input.Email + " or " + Input.phoneNumber + "</div><div><br/>The Speech Accessibility Project Team<br/>University of Illinois Urbana-Champaign</div>";

                        string to = _config["LSVTEmail"];

                        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                        if (_config["DeveloperMode"].Equals("Yes") || !"Production".Equals(environment))
                        {
                            to = _config["TestEmail"];
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
                {                   
                    ModelState.AddModelError(string.Empty, error.Description);
                    resultErrorList.Add(error.Description);

                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private Contributor PopulateContributor(IdentityUser user)
        {
            Contributor contributor = new Contributor();
            contributor.HelperEmail = Input.HelperEmail;
            contributor.HelperFirstName = Input.HelperFirstName;
            contributor.HelperLastName = Input.HelperLastName;
            contributor.FirstName = Input.firstName;
            contributor.MiddleName = Input.middleName;
            contributor.LastName = Input.lastName;
            contributor.StateResidence = Input.state;
            contributor.UnderstandSpeechInd = Input.understandSpeechInd;
            contributor.ParkinsonsInd = Input.parkinsonsInd;
            contributor.IdentityUser = user;
            contributor.EighteenOrOlderInd = Input.eighteenOrOlderInd;
            contributor.Status = new ContributorStatus { Id = 1 };
            contributor.ContactLSVT = Input.ContactLSVT;
            contributor.EmailAddress = Input.Email;
            contributor.PhoneNumber = Input.phoneNumber;
            return contributor;
        }
    }
}
