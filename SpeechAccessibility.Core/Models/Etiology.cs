using System;
using System.Collections.Generic;

namespace SpeechAccessibility.Core.Models
{
    public class Etiology
    {
        public Etiology()
        {
            Contributor = new HashSet<Contributor>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Active { get; set; }
        public int? DisplayOrder { get; set; }
        public int FirstGiftCard { get; set; }
        public int SecondGiftCard { get; set; }
        public int ThirdGiftCard { get; set; }
        public DateTime CreateTS { get; set; }
        public DateTime UpdateTS { get; set; }
        public string RegisterRequired { get; set; }
        public string Acronym { get; set; }
        public ICollection<Contributor> Contributor { get; set; }
    }
}
