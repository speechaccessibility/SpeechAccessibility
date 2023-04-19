using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SpeechAccessibility.Models;
using Microsoft.AspNetCore.WebUtilities;
using SpeechAccessibility.Data;
using System.Text;
using Microsoft.AspNetCore.Http;
using UAParser;

namespace SpeechAccessibility.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IdentityContext _identityContext;

        public LoginModel(SignInManager<IdentityUser> signInManager, 
            ILogger<LoginModel> logger,
            UserManager<IdentityUser> userManager,
            IdentityContext identityContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _identityContext = identityContext;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            var email = Request.Cookies["SAAEmail"];

            if (!string.IsNullOrEmpty(email))
            {
                Input = new InputModel { Email = email };
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                  
                    Task<IdentityUser> current_User = _signInManager.UserManager.FindByNameAsync(Input.Email);
                    IdentityUser user = current_User.Result;
                    string id = user.Id;

                    if (Input.RememberMe)

                    {
                        var cookieOptions = new CookieOptions
                        {
                            Expires = DateTime.UtcNow.AddDays(30)
                        };

                        Response.Cookies.Append("SAAEmail", Input.Email, cookieOptions);
                    }
                                     

                    //Retrieve the contributor that is linked to the signed in user ID
                    bool changePassword = _identityContext.Contributor
                       .Where(o => o.IdentityUser.Id == id).Select(c => c.ChangePassword).FirstOrDefault();

                    if (changePassword)
                    {
                        string code = await _userManager.GeneratePasswordResetTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                       
                        return RedirectToPage("./ResetPassword", new { code = code, Email = Input.Email, LoggedIn=true});
                    }
                    _logger.LogInformation("User logged in.");

                    InsertLoginSession(id);

                    return RedirectToAction("RecordPrompt");
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private void InsertLoginSession(string id)
        {
            Contributor contributor = _identityContext.Contributor
               .Where(o => o.IdentityUser.Id == id)
               .FirstOrDefault();

            string userAgent = Request.Headers["User-Agent"].ToString();

            var uaParser = Parser.GetDefault();

            ClientInfo c = uaParser.Parse(userAgent);

            string osVersion = c.OS.Major;
            if (c.OS.Minor != null)
            {
                osVersion += "." + c.OS.Minor;
            }

            string browserVersion = c.UA.Major;
            if (c.UA.Major != null)
            {
                browserVersion += "." + c.UA.Minor;
            }


            LoginSession session = new LoginSession
            {
                DeviceFamily = c.Device.Family,
                DeviceModel = c.Device.Model,
                OperatingSystem = c.OS.Family,
                OperatingSystemVersion = osVersion,
                OperatingSystemPatch = c.OS.Patch,
                Browser = c.UA.Family,
                BrowserVersion = browserVersion,
                BrowserPatch = c.UA.Patch,
                LoginTS = DateTime.Now,
                Contributor = contributor

            };

            _identityContext.LoginSession.Add(session);
            _identityContext.SaveChanges();

        }
    }
}
