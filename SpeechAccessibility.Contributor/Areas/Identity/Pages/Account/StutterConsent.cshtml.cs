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
using Microsoft.AspNetCore.Identity.UI.Services;
using SpeechAccessibility.Services;
using Newtonsoft.Json;

namespace SpeechAccessibility.Areas.Identity.Pages.Account
{
    public class StutterConsentModel : PageModel
    {
        private readonly IdentityContext _identityContext;
        private readonly IConfiguration _config;
        private readonly IMailService _emailSender;
        private static InputModel _inputModel;

        [Required]
        [BindProperty]
        public bool readAndUnderstandConsent
        {
            get; set;
        }
        [Required]
        [BindProperty]
        public bool eighteenConsent { get; set; }
        [Required]
        [BindProperty]
        public bool validEmailConsent { get; set; }
        [Required]
        [BindProperty]
        public bool loginConsent { get; set; }
        [Required]
        [BindProperty]
        public bool legalAgreementConsent { get; set; }

        [Required]
        [BindProperty]
        public bool financialCompensationConsent { get; set; }

        [Required]
        [BindProperty]
        public bool maxPaymentConsent { get; set; }

        [Required]
        [BindProperty]
        public bool withdrawalConsent { get; set; }

        [BindProperty]
        public bool shareSamplesConsent { get; set; }

        [BindProperty]
        public bool shareContactInfoConsent { get; set; }


        public StutterConsentModel(IdentityContext identityContext, IConfiguration config, IMailService emailSender)
        {
            _identityContext = identityContext;
            _config = config;
            _emailSender = emailSender;
        }


        public void OnGet()
        {
            _inputModel = JsonConvert.DeserializeObject<InputModel>((string)TempData["Input"]);
            Console.WriteLine(_inputModel.Email);
        }

        public IActionResult OnPostAsync()
        {
            List<string> consentNameList = new List<string>();

            if (!readAndUnderstandConsent)
            {
                ModelState.AddModelError("readAndUnderstandValidation", "You must check the box above to proceed.");
            }
            else
            {
                consentNameList.Add("Read and Understand Consent");
            }
            if (!eighteenConsent)
            {
                ModelState.AddModelError("eighteenValidation", "You must check the box above to proceed.");

            }
            else
            {
                consentNameList.Add("Eighteen Years Old Consent");
            }
            if (!validEmailConsent)
            {
                ModelState.AddModelError("emailValidation", "You must check the box above to proceed.");

            }
            else
            {
                consentNameList.Add("Valid Email Consent");
            }
            if (!loginConsent)
            {
                ModelState.AddModelError("loginValidation", "You must check the box above to proceed.");

            }
            else
            {
                consentNameList.Add("Login Consent");
            }
            if (!legalAgreementConsent)
            {
                ModelState.AddModelError("legalAgreementValidation", "You must check the box above to proceed.");

            }
            else
            {
                consentNameList.Add("Legal Agreement Consent");
            }
            if (!financialCompensationConsent)
            {
                ModelState.AddModelError("financialCompensationValidation", "You must check the box above to proceed");
            }
            else
            {
                consentNameList.Add("Financial Compensation Consent");
            }
            if (!maxPaymentConsent)
            {
                ModelState.AddModelError("maxPaymentValidation", "You must check the box above to proceed");
            }
            else
            {
                consentNameList.Add("Max Payment Consent");
            }


            if (!withdrawalConsent)
            {
                ModelState.AddModelError("withdrawalValidation", "You must check the box above to proceed");
            }
            else
            {
                consentNameList.Add("Withdrawal Consent");
            }
            if (shareSamplesConsent)
            {
                consentNameList.Add("Share Samples Consent");
            }

            if (shareContactInfoConsent)
            {
                consentNameList.Add("Share Contact Info Consent");
            }
            if (ModelState.IsValid)
            {
                Contributor contributor = PopulateContributor();
                _identityContext.Contributor.Add(contributor);

                _identityContext.Etiology.Remove(contributor.Etiology);
                _identityContext.SaveChanges();


                foreach (string name in consentNameList)
                {
                    Consent consent = new Consent
                    {
                        Version = _config["StutteringConsentVersion"],
                        Contributor = contributor,
                        Name = name,
                        ConsentType = "Participant"
                    };

                    _identityContext.Add(consent);
                    _identityContext.SaveChanges();
                }

                StutteringScreening stutterScreening = _inputModel.StutterScreening;
                stutterScreening.ContributorId = contributor.Id;
                _identityContext.StutteringScreening.Add(stutterScreening);

                foreach (SituationRating rating in _inputModel.SituationRating)
                {
                    rating.ContributorId = contributor.Id;
                    _identityContext.SituationRating.Add(rating);
                    _identityContext.SaveChanges();
                }



                string email = _inputModel.Email;
                string phone = _inputModel.phoneNumber;

                SendEnrollmentEmail(email);
                SendNotificationEmail(email, phone);

                return RedirectToAction("ApprovalRequired", new { etiologyId = 7 });
            }
            else
            {
                return Page();
            }
        }

        private void SendNotificationEmail(string email, string phone)
        {
            string message = "<div>Hello,</div><br/><div>A potential Speech Accessibility Project participant, " + _inputModel.firstName + ", has requested an assessment. You may contact them at " + email;
                if (phone != null)
            { 
                message+= " or " + phone ;
            } 
                message+= ".</div><div><br/>The Speech Accessibility Project Team<br/>University of Illinois Urbana-Champaign</div>";

            string to = _config["StutteringEmail"];

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (_config["DeveloperMode"].Equals("Yes") || !"Production".Equals(environment))
            {
                to = _config["TestEmail"];
                string testMessage = "<p><strong>This email was sent in testing mode.</strong></p>";
                message = testMessage + message;
            }

            _emailSender.SendEmailAsync(to, "Assessment Request", message);
        }

        private void SendEnrollmentEmail(string emailAddress)
        {

            string message = "<p>Thank you for your interest in the Speech Accessibility Project. AImpower.org will review the information you provided and determine if you are a good fit for our study. You will receive another email in about 7-10 days to let you know whether you can participate in this study.</p>" +
                "<p>Thank you for your time and we will be in touch soon!</p><p>The Speech Accessibility Project Team</p>";


            string subject = "Thank you for enrolling in the Speech Accessibility Project study!";

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (_config["DeveloperMode"].Equals("Yes") || !"Production".Equals(environment))
            {
                emailAddress = _config["TestEmail"];
            }

            _emailSender.SendEmailAsync(emailAddress, subject, message);
        }

        private Contributor PopulateContributor()
        {
            Contributor contributor = new Contributor();
            contributor.FirstName = _inputModel.firstName;
            contributor.MiddleName = _inputModel.middleName;
            contributor.LastName = _inputModel.lastName;
            contributor.StateResidence = _inputModel.state;
            contributor.Etiology = new Etiology { Id = _inputModel.etiologyId };
            contributor.EighteenOrOlderInd = _inputModel.eighteenOrOlderInd;
            contributor.StatusId = 1;
            contributor.EmailAddress = _inputModel.Email;
            contributor.PhoneNumber = _inputModel.phoneNumber;
            contributor.OtherEtiologyText = _inputModel.otherText;
            contributor.BirthYear = _inputModel.BirthYear;
            contributor.ReferenceSource = _inputModel.ReferenceSource;
            contributor.Country = _inputModel.Country;
            return contributor;
        }
    }

     


    public class InputModel
    {
        public string otherText { get; set; }


        [Required]
        [Display(Name = "EighteenOrOlderInd")]
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
        [MinLength(10, ErrorMessage = "Phone number must be 10 digits")]
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

        [Required]
        [MaxLength(150)]
        [Display(Name = "Reference Source")]
        public string ReferenceSource { get; set; }

        public string Country { get; set; }

        public string DuplicateEmailInd { get; set; }

        public StutteringScreening StutterScreening { get; set; }

        public List<SituationRating> SituationRating { get; set; }
    }
}
