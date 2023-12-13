using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class ContributorViewRepository : Repository<SpeechAccessibilityDbContext, ContributorView>, IContributorViewRepository
    {
        public ContributorViewRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
