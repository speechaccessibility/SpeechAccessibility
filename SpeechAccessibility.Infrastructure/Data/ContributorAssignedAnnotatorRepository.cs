using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class ContributorAssignedAnnotatorRepository : Repository<SpeechAccessibilityDbContext,ContributorAssignedAnnotator>, IContributorAssignedAnnotatorRepository
    {
        public ContributorAssignedAnnotatorRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
