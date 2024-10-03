using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
    public class AspNetUsers
    {
        public AspNetUsers()
        {
            Contributor = new HashSet<Contributor>();
        }


        public string Id { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public string PasswordHash { get; set; }

        public  ICollection<Contributor> Contributor { get; set; }
    }
}
