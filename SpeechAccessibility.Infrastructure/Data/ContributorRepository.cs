using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class ContributorRepository : Repository<SpeechAccessibilityContributorDbContext,Contributor>, IContributorRepository
    {
        public ContributorRepository(SpeechAccessibilityContributorDbContext speechAccessibilityContributorDbContext) : base(speechAccessibilityContributorDbContext)
        {
        }
    }
}
