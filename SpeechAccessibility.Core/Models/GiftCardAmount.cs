using System.ComponentModel.DataAnnotations.Schema;

namespace SpeechAccessibility.Core.Models
{
    public  class GiftCardAmount
    {
        public int Id { get; set; }
        public int EtiologyId { get; set; }
        public int? PromptCategoryId { get; set; }
        public int FirstGiftCard { get; set; }
        public int SecondGiftCard { get; set; }
        public int ThirdGiftCard { get; set; }
        [NotMapped]
        public string EtiologyName { get; set; }
        [NotMapped]
        public string PromptCategoryName { get; set; }
    }
}
