using SpeechAccessibility.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpeechAccessibility.Data.Entities
{
    public class ContributorDetails
    {
        public int Id { get; set; }
        //[Required]
        //public string RacialGroup { get; set; }
        [MaxLength(20)]
        [Required]
        public string HispanicOrLatino {get;set;}
        [MaxLength(20)]
        [Required]
        public string Gender { get; set; }
        [MaxLength(20)]
        [Required]
        public string Age { get; set; }
        [MaxLength(4)]
        [Required]
        public string State { get; set; }
        [MaxLength(20)]
        [Required]
        public string ConverseInEnglish { get; set; }
        [MaxLength(20)]
        public string ConverseInOtherLanguageAge { get; set; }
        public string OtherLanguage { get; set; }
        [MaxLength(30)]
        [Required]
        public string FamiliarPeopleAtHomeRating { get; set; }
        [MaxLength(30)]
        [Required]
        public string UnfamiliarPeopleAtHomeRating { get; set; }
        [MaxLength(30)]
        [Required]
        public string FamiliarPeopleOnPhoneRating { get; set; }
        [MaxLength(30)]
        [Required]
        public string UnfamiliarPeopleOnPhoneRating { get; set; }
       
        [MaxLength(30)]
        [Required]
        public string NoisyEnvironmentRating { get; set; }  
       
        [MaxLength(30)]
        [Required]
        public string TravelInCarRating { get; set; }   
        
        [MaxLength(30)]
        [Required]
        public string LongConversationRating { get; set; }  
        [MaxLength(30)]
        [Required]
        public string UpsetOrAngryRating { get; set; }  
        [MaxLength(30)]
        [Required]
        public string FrustratedBySpeechRating { get; set; }
        [MaxLength(30)]
        [Required]
        public string RelyOnOthersRating { get; set; }      
        [MaxLength(30)]
        [Required]
        public string RepeatMyselfRating { get; set; } 
        [MaxLength(30)]
        [Required]
        public string DifficultyHearingRating { get; set; }   
        [MaxLength(30)]
        [Required]
        public string AvoidSpeechWhenTiredRating { get; set; }
        [MaxLength(30)]
        [Required]
        public string ImpactSocialActivitiesRating { get; set; }
        
        [MaxLength(20)]
        [Required]
        public string SpeechChange { get; set; }

        public string SpeechChangeDescription { get; set; }

        public string Retired { get; set; }

        public string Occupation { get; set; }

        public string Education { get; set; }

        [Required]
        public Contributor Contributor { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateTS { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateTS { get; set; }


    }
}
