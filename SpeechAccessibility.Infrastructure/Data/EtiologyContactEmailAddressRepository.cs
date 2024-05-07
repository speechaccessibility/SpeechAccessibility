using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class EtiologyContactEmailAddressRepository : Repository<SpeechAccessibilityDbContext, EtiologyContactEmailAddress>, IEtiologyContactEmailAddressRepository
    {
        public EtiologyContactEmailAddressRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
