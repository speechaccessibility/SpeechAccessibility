using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class EmailLoggingRepository : Repository<SpeechAccessibilityDbContext, EmailLogging>, IEmailLoggingRepository
    {
        public EmailLoggingRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
