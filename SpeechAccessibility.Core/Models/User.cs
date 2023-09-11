using System;
using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
    public class User
    {
        public User()
        {
            ContributorAssignedAnnotator = new HashSet<ContributorAssignedAnnotator>();
            UserSubRole = new HashSet<UserSubRole>();
        }

        public int Id { get; set; }
        public int RoleId { get; set; }
        public string NetId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? UpdateTS { get; set; }
        public string UpdateBy { get; set; }
        public string Active { get; set; }
        public  Role Role { get; set; }
        public  ICollection<ContributorAssignedAnnotator> ContributorAssignedAnnotator { get; set; }
        public  ICollection<UserSubRole> UserSubRole { get; set; }
    }
}
