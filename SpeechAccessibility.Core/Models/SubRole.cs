using System;
using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
    public  class SubRole
    {
        public SubRole()
        {
            UserSubRole = new HashSet<UserSubRole>();
        }

        public int Id { get; set; }
        public int RoleId { get; set; }
        public int EtiologyId { get; set; }
        public DateTime UpdateTS { get; set; }
        public string InUsed { get; set; }

        public  Etiology Etiology { get; set; }
        public  Role Role { get; set; }
        public  ICollection<UserSubRole> UserSubRole { get; set; }
    }
}
