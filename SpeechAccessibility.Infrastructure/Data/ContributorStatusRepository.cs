using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class ContributorStatusRepository : Repository<SpeechAccessibilityContributorDbContext,ContributorStatus>, IContributorStatusRepository
    {
        public ContributorStatusRepository(SpeechAccessibilityContributorDbContext speechAccessibilityContributorDbContext) : base(speechAccessibilityContributorDbContext)
        {
        }
    }
}
