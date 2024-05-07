using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class ContributorAssignedListRepository : Repository<SpeechAccessibilityDbContext, ContributorAssignedList>, IContributorAssignedListRepository
    {
        public ContributorAssignedListRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
