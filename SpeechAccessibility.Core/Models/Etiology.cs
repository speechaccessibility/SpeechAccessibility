using System;
using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
    public  class Etiology
    {
        public Etiology()
        {
            ConsentVersion = new HashSet<ConsentVersion>();
            Contributor = new HashSet<Contributor>();
            SubRole = new HashSet<SubRole>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Active { get; set; }
        public int? DisplayOrder { get; set; }
        public DateTime CreateTS { get; set; }
        public DateTime UpdateTS { get; set; }
        public ICollection<ConsentVersion> ConsentVersion { get; set; }
        public ICollection<Contributor> Contributor { get; set; }
        public ICollection<SubRole> SubRole { get; set; }

      

    }
}
