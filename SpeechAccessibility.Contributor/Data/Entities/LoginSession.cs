using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace SpeechAccessibility.Models
{
    public class LoginSession
    {
        public int Id { get; set; }
        public Contributor Contributor { get; set; }

        public string Browser { get; set; }

        public string BrowserVersion { get; set; }

        public string BrowserPatch { get; set; }

        public string OperatingSystem { get; set; } 

        public string OperatingSystemVersion { get; set; }

        public string OperatingSystemPatch { get; set; }
       
        public string DeviceFamily { get; set; }

        public string DeviceModel { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime LoginTS { get; set; }
    }
}
