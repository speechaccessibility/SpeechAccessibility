using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Models
{
    public class ADMemberViewModel 
    {
        public int Id { get; set; }

        [Display(Name = "Role")]
        public int RoleId { get; set; }

        [Display(Name = "Etiology")]
        public int SubRoleId { get; set; }
        public bool HasSubRole { get; set; }
        public string RoleName { get; set; }
        public string ADGroupName { get; set; }
        public string NetId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Active { get; set; }
        public string ExistingADName { get; set; }
        public List<SubRole> AssignedSubRoles { set; get; }
        //public List<SelectListItem> AssignedSubRoles { set; get; }
        public List<SubRole> AvailableSubRoles { set; get; }
        //public List<SelectListItem> AvailableSubRoles { set; get; }
        public List<SelectListItem> Roles { set; get; }
        public List<SelectListItem> SubRoles { set; get; }


    }
}
