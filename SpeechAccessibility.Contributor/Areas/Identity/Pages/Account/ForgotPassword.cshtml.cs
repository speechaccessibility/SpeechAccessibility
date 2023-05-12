using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SpeechAccessibility.Models;
using SpeechAccessibility.Services;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace SpeechAccessibility.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMailService _emailSender;
        private readonly IConfiguration _config;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IMailService emailSender, IConfiguration config)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _config = config;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                string fileLocation = _config["ErrorLocation"] + date + "SpeechAccessibility.txt";

                Directory.CreateDirectory(Path.GetDirectoryName(fileLocation));

                var user = await _userManager.FindByEmailAsync(Input.Email);
                //if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                if (user == null)
                 {
                    using (StreamWriter writer = new StreamWriter(fileLocation, true))
                    {
                        string error = DateTime.Now.ToString() + " " + Input.Email + " not found. Reset password email not sent.";
                        writer.WriteLine(error);
                        writer.Close();
                    }
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                using (StreamWriter writer = new StreamWriter(fileLocation, true))
                {
                    string message = DateTime.Now.ToString() + " Sending reset password email to " + Input.Email;
                    writer.WriteLine(message);
                    writer.Close();
                }

                await _emailSender.SendEmailAsync(
                   Input.Email,
                   "Reset Password",
                   $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
               
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
