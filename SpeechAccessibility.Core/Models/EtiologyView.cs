using System;

namespace SpeechAccessibility.Core.Models
{
    public  class EtiologyView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Active { get; set; }
        public int? DisplayOrder { get; set; }
        public string Acronym { get; set; }
        public int FirstGiftCard { get; set; }
        public int SecondGiftCard { get; set; }
        public int ThirdGiftCard { get; set; }
        public DateTime CreateTS { get; set; }
        public DateTime UpdateTS { get; set; }

      


    }
}
