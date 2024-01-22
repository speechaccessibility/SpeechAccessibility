using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace SpeechAccessibility.Areas.Identity.Pages.Account
{
    public class OpenEndedInstructionsModel : PageModel
    {
        public string blockCount { get; set; }

        public void OnGet(string totalBlockCount)
        {
            blockCount = totalBlockCount;
        }

        public async Task<IActionResult> OnPost()
        {

            return RedirectToAction("RecordPrompt", new {displayedMessageForCurrentBlock=true});
        }
    }
}
