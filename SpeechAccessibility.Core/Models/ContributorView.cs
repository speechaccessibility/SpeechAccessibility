﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpeechAccessibility.Core.Models
{
    public class ContributorView
    {
        public Guid Id { get; set; }
        public string UnderstandSpeechInd { get; set; }
        public int EtiologyId { get; set; }
        public string EighteenOrOlderInd { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string HelperInd { get; set; }
        public string HelperEmail { get; set; }
        public string HelperFirstName { get; set; }
        public string HelperLastName { get; set; }
        public string StateResidence { get; set; }
        public string IdentityUserId { get; set; }
        public int StatusId { get; set; }
        public int? SubStatusId { get; set; }
        public DateTime CreateTS { get; set; }
        public DateTime? UpdateTS { get; set; }
        public string Comments { get; set; }
        public bool? ChangePassword { get; set; }
        public bool? ContactLSVT { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? ApproveTS { get; set; }
        public string ApproveDenyBy { get; set; }
        public string OtherEtiologyText { get; set; }
        public string BirthYear { get; set; }
        public string ParkinsonsInd { get; set; }
        public string HelperPhoneNumber { get; set; }
        public string StatusName { get; set; }
        public string SubStatusName { get; set; }
        public string EtiologyName { get; set; }
        public int? LegalGuardianId { get; set; }
        public string RegisterRequired { get; set; }
        public string LegalGuardianFirstName { get; set; }
        public string LegalGuardianLastName { get; set; }
        public string LegalGuardianEmail { get; set; }
        public string LegalGuardianPhoneNumber { get; set; }
        public string Country { get; set; }
        public string PaymentType { get; set; }
        public string ReferenceSource { get; set; }

        [NotMapped]
        public int SubRole { get; set; }
        [NotMapped]
        public string LegalGuardianInd { get; set; }

        [NotMapped]
        public string HelperNotPaid { get; set; }
    }
}
