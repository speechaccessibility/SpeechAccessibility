using System;

namespace SpeechAccessibility.Core.Models
{
    public class UserSubRole
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SubRoleId { get; set; }
        public DateTime? UpdateTS { get; set; }

        public  SubRole SubRole { get; set; }
        public  User User { get; set; }
    }
}
