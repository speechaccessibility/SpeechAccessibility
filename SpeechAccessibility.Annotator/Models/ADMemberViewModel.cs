using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Models
{
    public class ADMemberViewModel : BaseEntity
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string ADGroupName { get; set; }
        public string NetId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string ExistingADName { get; set; }
        public List<SelectListItem> Roles { set; get; }

    }
}
