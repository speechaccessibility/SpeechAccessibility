using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SpeechAccessibility.Areas.Identity.Pages.Account
{
    public class DiagnosisRegisterModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Diagnosis")]
            public string etiologyId { get; set; }
        }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                if ("1".Equals(Input.etiologyId))
                {
                    return RedirectToPage("Register", new { etiology = Input.etiologyId });
                }
                else if ("2".Equals(Input.etiologyId))
                {
                    return RedirectToPage("DSPreRegister", new { etiology = Input.etiologyId });
                }
                else
                {
                    return RedirectToPage("InvalidDiagnosis");
                }


            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }

}
