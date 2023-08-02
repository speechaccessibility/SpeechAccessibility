using EntityFrameworkCore.EncryptColumn.Attribute;
using Microsoft.AspNetCore.Identity;
using SpeechAccessibility.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;


namespace SpeechAccessibility.Models
{
    public class Contributor
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [MaxLength(3)]      
        public string UnderstandSpeechInd { get; set; }

        [MaxLength(3)]
        [Required]
        public string ParkinsonsInd { get; set; }

        [MaxLength(3)]
        [Required]
        public string EighteenOrOlderInd { get; set; }

        [MaxLength(50)]
        [Required]
        public string FirstName { get; set; }

        [MaxLength(30)]
        public string MiddleName { get; set; }

        [MaxLength(50)]
        [Required]
        public string LastName { get; set; }

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [MaxLength(256)]
        public string EmailAddress { get; set; }


        [MaxLength(3)]
         public string HelperInd { get; set; }

        [MaxLength(175)]
        public string HelperEmail { get; set; }

        [MaxLength(50)]
        public string HelperFirstName { get; set; }
        [MaxLength(50)]
        public string HelperLastName { get; set; }

        [MaxLength(10)]
        public string HelperPhoneNumber { get; set; }

        [MaxLength(4)]
        [Required]
        public string StateResidence { get; set; }
        [Required]
        public IdentityUser IdentityUser { get; set; }
        public List<Consent> Consent { get; set; }

        public ContributorStatus Status { get; set; }   
        
        public bool ChangePassword { get; set; }

        public bool ContactLSVT { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateTS { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateTS { get; set; }

        public string BirthYear { get; set; }
    }
}
