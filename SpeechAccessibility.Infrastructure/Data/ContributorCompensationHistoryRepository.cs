using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class ContributorCompensationHistoryRepository : Repository<SpeechAccessibilityDbContext, ContributorCompensationHistory>, IContributorCompensationHistoryRepository
    {
        public ContributorCompensationHistoryRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
