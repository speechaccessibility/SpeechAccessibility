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

        public Etiology Etiology { get; set; }

        //[MaxLength(3)]
        //[Required]
        //public string ParkinsonsInd { get; set; }

        public string OtherEtiologyText { get; set; }

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
        public string StateResidence { get; set; }

        public IdentityUser IdentityUser { get; set; }
        public List<Consent> Consent { get; set; }

        public int StatusId { get; set; }   
        
        public bool ChangePassword { get; set; }

        public bool ContactLSVT { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateTS { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateTS { get; set; }

        public string BirthYear { get; set; }

        public string CurrentAge { get; set; }

        public string DiagnosisAge { get; set; }

        public string LengthOfDiagnosis { get; set; }

        public int PromptCategoryId { get; set; }

        public string TimeZone { get; set; }

        public string Country { get; set; }

        [MaxLength (150)]
        public string ReferenceSource { get; set; }

    }
}
