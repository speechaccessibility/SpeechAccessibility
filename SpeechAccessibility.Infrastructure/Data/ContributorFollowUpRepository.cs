using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class ContributorFollowUpRepository : Repository<SpeechAccessibilityDbContext, ContributorFollowUp>, IContributorFollowUpRepository
    {
        public ContributorFollowUpRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
