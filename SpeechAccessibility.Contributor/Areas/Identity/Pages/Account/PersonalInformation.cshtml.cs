using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using SpeechAccessibility.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SpeechAccessibility.Areas.Identity.Pages.Account
{
    public class PersonalInformationModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        public List<String> unqualifiedStates = new List<String>
        {
            "IL",
            "TX",
            "WA",
            "None"
        };

        public List<SelectListItem> stateList { get; } = new List<SelectListItem>
         {
                    new SelectListItem { Value = "AL", Text = "Alabama" },
                    new SelectListItem { Value = "AK", Text = "Alaska" },
                    new SelectListItem { Value = "AZ", Text = "Arizona" },
                    new SelectListItem { Value = "AR", Text = "Arkansas" },
                    new SelectListItem { Value = "CA", Text = "California" },
                    new SelectListItem { Value = "CO", Text = "Colorado" },
                    new SelectListItem { Value = "CT", Text = "Connecticut" },
                    new SelectListItem { Value = "DE", Text = "Delaware" },
                    new SelectListItem { Value = "FL", Text = "Florida" },
                    new SelectListItem { Value = "GA", Text = "Georgia" },
                    new SelectListItem { Value = "HI", Text = "Hawaii" },
                    new SelectListItem { Value = "ID", Text = "Idaho" },
                    new SelectListItem { Value = "IL", Text = "Illinois" },
                    new SelectListItem { Value = "IN", Text = "Indiana" },
                    new SelectListItem { Value = "IA", Text = "Iowa" },
                    new SelectListItem { Value = "KS", Text = "Kansas" },
                    new SelectListItem { Value = "KY", Text = "Kentucky" },
                    new SelectListItem { Value = "LA", Text = "Louisiana" },
                    new SelectListItem { Value = "ME", Text = "Maine" },
                    new SelectListItem { Value = "MD", Text = "Maryland" },
                    new SelectListItem { Value = "MA", Text = "Massachusetts" },
                    new SelectListItem { Value = "MI", Text = "Michigan" },
                    new SelectListItem { Value = "MN", Text = "Minnesota" },
                    new SelectListItem { Value = "MS", Text = "Mississippi" },
                    new SelectListItem { Value = "MO", Text = "Missouri" },
                    new SelectListItem { Value = "MT", Text = "Montana" },
                    new SelectListItem { Value = "NC", Text = "North Carolina" },
                    new SelectListItem { Value = "ND", Text = "North Dakota" },
                    new SelectListItem { Value = "NE", Text = "Nebraska" },
                    new SelectListItem { Value = "NV", Text = "Nevada" },
                    new SelectListItem { Value = "NH", Text = "New Hampshire" },
                    new SelectListItem { Value = "NJ", Text = "New Jersey" },
                    new SelectListItem { Value = "NM", Text = "New Mexico" },
                    new SelectListItem { Value = "NY", Text = "New York" },
                    new SelectListItem { Value = "OH", Text = "Ohio" },
                    new SelectListItem { Value = "OK", Text = "Oklahoma" },
                    new SelectListItem { Value = "OR", Text = "Oregon" },
                    new SelectListItem { Value = "PA", Text = "Pennsylvania" },
                    new SelectListItem { Value = "RI", Text = "Rhode Island" },
                    new SelectListItem { Value = "SC", Text = "South Carolina" },
                    new SelectListItem { Value = "SD", Text = "South Dakota" },
                    new SelectListItem { Value = "TN", Text = "Tennessee" },
                    new SelectListItem { Value = "TX", Text = "Texas" },
                    new SelectListItem { Value = "UT", Text = "Utah" },
                    new SelectListItem { Value = "VT", Text = "Vermont" },
                    new SelectListItem { Value = "VA", Text = "Virginia" },
                    new SelectListItem { Value = "WA", Text = "Washington" },
                    new SelectListItem { Value = "WV", Text = "West Virginia" },
                    new SelectListItem { Value = "WI", Text = "Wisconsin" },
                    new SelectListItem { Value = "WY", Text = "Wyoming" },
                    new SelectListItem {Value ="None", Text="None of the above"}
                };

        public List<SelectListItem> yearList { get; } = getYearList();
           
       private static List<SelectListItem> getYearList()
        {
            List<SelectListItem> yearList = new List<SelectListItem>();
            SelectListItem unknown = new SelectListItem { Value = "0", Text = "Unknown" };
            yearList.Add(unknown);
            for (int i = DateTime.Now.Year; i >=1900 ; i--)
            {
                SelectListItem item = new SelectListItem { Value = i.ToString(), Text = i.ToString() };
                yearList.Add(item);
            }
            return yearList;
        }

        public class InputModel
        {
            [Required]
            [Display(Name = "Speech Indicator")]
            public string understandSpeechInd { get; set; }

            [Required]
            [Display(Name = "Parkinson's Disease Indicator")]
            public string parkinsonsInd { get; set; }
            [Required]
            [Display(Name = "Eighteen Indicator")]
            public string eighteenOrOlderInd { get; set; }            

            [Required]
            [Display(Name = "State")]
            public string state { get; set; }


        }


        public void OnGet()
        {

        }

        public IActionResult OnPost()
        {          
            if (ModelState.IsValid)
            {
                var personalInformation = new InputModel 
                { 
                    state = Input.state,
                    understandSpeechInd= Input.understandSpeechInd,
                    parkinsonsInd=Input.parkinsonsInd,
                    eighteenOrOlderInd=Input.eighteenOrOlderInd,
                };

                if (unqualifiedStates.Contains(Input.state) || "No".Equals(Input.eighteenOrOlderInd))
                {
                    return RedirectToPage("./Unqualified");
                }

                return RedirectToPage("./Register", personalInformation);
            }
            else
            {
                return Page();
            }


        }
    }
}
