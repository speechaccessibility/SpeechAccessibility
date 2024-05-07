using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class RegisterLinkRepository : Repository<SpeechAccessibilityDbContext, RegisterLink>, IRegisterLinkRepository
    {
        public RegisterLinkRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
