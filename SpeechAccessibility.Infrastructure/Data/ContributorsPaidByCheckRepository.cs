using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class ContributorsPaidByCheckRepository : Repository<SpeechAccessibilityDbContext, ContributorsPaidByCheck>, IContributorsPaidByCheckRepository
    {
        public ContributorsPaidByCheckRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
