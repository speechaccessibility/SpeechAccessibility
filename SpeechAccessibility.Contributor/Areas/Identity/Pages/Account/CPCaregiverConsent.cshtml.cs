using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using SpeechAccessibility.Data;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using SpeechAccessibility.Models;
using SpeechAccessibility.Data.Entities;

namespace SpeechAccessibility.Areas.Identity.Pages.Account
{

    public class CPCaregiverConsentModel : PageModel
    {
        private readonly IdentityContext _identityContext;
        private readonly IConfiguration _config;

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

        public CPCaregiverConsentModel(IdentityContext identityContext, IConfiguration config)
        {
            _identityContext = identityContext;
            _config = config;
        }

        public void OnGet()
        {
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
                Guid contributorId = new Guid(TempData["contributorId"].ToString());
                Contributor contributor = _identityContext.Contributor.Where(c => c.Id == contributorId).FirstOrDefault();

                foreach (string name in consentNameList)
                {
                    Consent consent = new Consent
                    {
                        Version = _config["CPCaregiverConsentVersion"],
                        Contributor = contributor,
                        Name = name,
                        ConsentType= "Caregiver"
                    };

                    _identityContext.Add(consent);
                    _identityContext.SaveChanges();
                }

                int legalGuardianCount = _identityContext.LegalGuardian.Where(c => c.ContributorId == contributor.Id).Count();

                if (legalGuardianCount > 0)
                {
                    return RedirectToPage("CPAssent");
                }

                return RedirectToAction("RecordPrompt");
            }
            else
            {
                return Page();
            }
        }
    }

}
