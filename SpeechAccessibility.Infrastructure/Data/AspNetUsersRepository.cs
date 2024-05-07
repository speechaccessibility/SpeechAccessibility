using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class AspNetUsersRepository : Repository<SpeechAccessibilityContributorDbContext, AspNetUsers>, IAspNetUsersRepository
    {
        public AspNetUsersRepository(SpeechAccessibilityContributorDbContext speechAccessibilityContributorDbContext) : base(speechAccessibilityContributorDbContext)
        {
        }
    }
}
