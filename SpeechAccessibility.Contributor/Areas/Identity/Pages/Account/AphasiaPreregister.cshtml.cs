using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SpeechAccessibility.Areas.Identity.Pages.Account
{
    public class AphasiaPreRegisterModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel {

            [Required]
            [Display(Name ="Aphasia Indicator")]
            public string AphasiaInd { get; set; }
        }
        public IActionResult OnGet(int etiology)
        {
            if (etiology == 0)
            {
                return RedirectToPage("./DiagnosisRegister");
            }
            return Page();  
        }
        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                return RedirectToPage("AphasiaRegister", new { aphasiaInd = Input.AphasiaInd});
            }
            return Page();
        }
    }
}
