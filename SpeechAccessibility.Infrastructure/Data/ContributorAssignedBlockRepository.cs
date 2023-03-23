using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class ContributorAssignedBlockRepository : Repository<SpeechAccessibilityDbContext,ContributorAssignedBlock>, IContributorAssignedBlockRepository
    {
        public ContributorAssignedBlockRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
