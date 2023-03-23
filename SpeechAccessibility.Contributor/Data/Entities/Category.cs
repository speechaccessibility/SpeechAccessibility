using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace SpeechAccessibility.Data.Entities
{
    public class Category
    {
        public int Id { get; set; }

        public string Description { get; set; }    

        public string Active { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateTS { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateTS { get; set; }
    }
}
