using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class UserSubRoleRepository : Repository<SpeechAccessibilityDbContext,UserSubRole>, IUserSubRoleRepository
    {
        public UserSubRoleRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
