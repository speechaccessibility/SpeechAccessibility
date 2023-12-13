using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class ApprovedDeniedContributorRepository : Repository<SpeechAccessibilityDbContext, ApprovedDeniedContributor>, IApprovedDeniedContributorRepository
    {
        public ApprovedDeniedContributorRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
