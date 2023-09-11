using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class ContributorSubStatusRepository : Repository<SpeechAccessibilityContributorDbContext,ContributorSubStatus>, IContributorSubStatusRepository
    {
        public ContributorSubStatusRepository(SpeechAccessibilityContributorDbContext speechAccessibilityContributorDbContext) : base(speechAccessibilityContributorDbContext)
        {
        }
    }
}
