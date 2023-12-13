using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SpeechAccessibility.Areas.Identity.Pages.Account
{
    public class DSPreRegisterModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel {

            [Required]
            [Display(Name ="Down Syndrome Indicator")]
            public string DownSyndromeInd { get; set; }
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
                return RedirectToPage("DSRegister", new { downSyndromeInd = Input.DownSyndromeInd});
            }
            return Page();
        }
    }
}
