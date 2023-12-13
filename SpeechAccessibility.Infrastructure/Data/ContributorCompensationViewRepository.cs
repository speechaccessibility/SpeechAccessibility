using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class ContributorCompensationViewRepository : Repository<SpeechAccessibilityDbContext, ContributorCompensationView>, IContributorCompensationViewRepository
    {
        public ContributorCompensationViewRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
