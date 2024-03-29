﻿using System;
using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
    public class Role
    {
        public Role()
        {
            SubRole = new HashSet<SubRole>();
            User = new HashSet<User>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string ADGroupName { get; set; }
        public string Description { get; set; }
        public bool HasSubRole { get; set; }
        public DateTime UpdateTS { get; set; }
        public string InUsed { get; set; }

        public  ICollection<SubRole> SubRole { get; set; }
        public  ICollection<User> User { get; set; }

    }
}
