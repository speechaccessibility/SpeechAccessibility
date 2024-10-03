using System;

namespace SpeechAccessibility.Core.Models
{
    public  class LegalGuardian
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Guid ContributorId { get; set; }
        public DateTime CreateTs { get; set; }
        public DateTime UpdateTs { get; set; }

        public  Contributor Contributor { get; set; }
    }
}
