using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class ContributorCompensationRepository : Repository<SpeechAccessibilityDbContext, ContributorCompensation>, IContributorCompensationRepository
    {
        public ContributorCompensationRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
