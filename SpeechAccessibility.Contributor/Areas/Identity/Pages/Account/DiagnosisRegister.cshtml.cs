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

            [MaxLength(100)]
            public string otherEtiologyDescription { get; set; }
        }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            if ("5".Equals(Input.etiologyId) && string.IsNullOrEmpty(Input.otherEtiologyDescription))
            {
                ModelState.AddModelError("otherDiagnosisValidation", "Condition that causes speech disability required.");
            }

            if (ModelState.IsValid)
            {              
                //if ("1".Equals(Input.etiologyId))
                //{
                //    return RedirectToPage("Register", new { etiology = Input.etiologyId });
                //}
                if ("2".Equals(Input.etiologyId))
                {
                    return RedirectToPage("DSPreRegister", new { etiology = Input.etiologyId});
                }
                else if ("3".Equals(Input.etiologyId) || "5".Equals(Input.etiologyId))
                {
                    return RedirectToPage("CPRegister", new { etiology = Input.etiologyId, otherEtiologyDescription = Input.otherEtiologyDescription });
                }
                else if ("4".Equals(Input.etiologyId))
                {
                    return RedirectToPage("AphasiaPreRegister", new {etiology = Input.etiologyId});
                }
                else if ("6".Equals(Input.etiologyId))
                {
                    return RedirectToPage("ALSRegister", new { etiology = 6 });
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
