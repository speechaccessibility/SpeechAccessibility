using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class EtiologyRepository : Repository<SpeechAccessibilityContributorDbContext, Etiology>, IEtiologyRepository
    {
        public EtiologyRepository(SpeechAccessibilityContributorDbContext speechAccessibilityContributorDbContext) : base(speechAccessibilityContributorDbContext)
        {
        }
    }
}
