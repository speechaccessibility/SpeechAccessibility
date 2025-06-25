using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class LegalGuardianRepository : Repository<SpeechAccessibilityContributorDbContext, LegalGuardian>, ILegalGuardianRepository
    {
        public LegalGuardianRepository(SpeechAccessibilityContributorDbContext speechAccessibilityContributorDbContext) : base(speechAccessibilityContributorDbContext)
        {
        }
    }
}
