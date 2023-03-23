using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Models
{
    public class ApplicationUserViewModel : BaseEntity
    {
        public string NetId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string[] Roles { get; set; }
    }
}
