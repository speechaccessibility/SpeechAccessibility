using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class UserRepository : Repository<SpeechAccessibilityDbContext,User>, IUserRepository
    {
        public UserRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
