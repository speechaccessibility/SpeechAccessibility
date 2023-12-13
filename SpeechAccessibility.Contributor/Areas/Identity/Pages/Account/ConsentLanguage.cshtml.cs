using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SpeechAccessibility.Areas.Identity.Pages.Account
{
    public class ConsentLanguageModel : PageModel
    {
        [Required]
        [BindProperty]
        public string language { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                if ("English".Equals(language))
                {
                    return RedirectToPage("Consent");
                }
                else {
                    return RedirectToPage("SpanishConsent");
                }
            }

            return Page();
        }
    }
}
