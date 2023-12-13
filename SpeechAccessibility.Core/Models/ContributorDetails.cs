using System;
using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
    public class ContributorDetails
    {
        public ContributorDetails()
        {
            ContributorRace = new HashSet<ContributorRace>();
        }

        public int Id { get; set; }
        public string HispanicOrLatino { get; set; }
        public string Gender { get; set; }
        public string Age { get; set; }
        public string State { get; set; }
        public string ConverseInEnglish { get; set; }
        public string ConverseInOtherLanguageAge { get; set; }
        public string OtherLanguage { get; set; }
        public string FamiliarPeopleAtHomeRating { get; set; }
        public string UnfamiliarPeopleAtHomeRating { get; set; }
        public string FamiliarPeopleOnPhoneRating { get; set; }
        public string UnfamiliarPeopleOnPhoneRating { get; set; }
        public string NoisyEnvironmentRating { get; set; }
        public string TravelInCarRating { get; set; }
        public string LongConversationRating { get; set; }
        public string UpsetOrAngryRating { get; set; }
        public string FrustratedBySpeechRating { get; set; }
        public string RelyOnOthersRating { get; set; }
        public string RepeatMyselfRating { get; set; }
        public string DifficultyHearingRating { get; set; }
        public string AvoidSpeechWhenTiredRating { get; set; }
        public string ImpactSocialActivitiesRating { get; set; }
        public string SpeechChange { get; set; }
        public string SpeechChangeDescription { get; set; }
        public Guid ContributorId { get; set; }
        public DateTime CreateTS { get; set; }
        public DateTime UpdateTS { get; set; }

        public ContributorView Contributor { get; set; }
        public  ICollection<ContributorRace> ContributorRace { get; set; }
    }
}
