using System;
using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
    public class ConsentVersion
    {
        public ConsentVersion()
        {
            Consent = new HashSet<Consent>();
        }

        public int Id { get; set; }
        public int Version { get; set; }
        public int EtiologyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public  Etiology Etiology { get; set; }
        public  ICollection<Consent> Consent { get; set; }
    }
}
