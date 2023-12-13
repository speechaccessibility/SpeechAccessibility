using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class EtiologyViewRepository : Repository<SpeechAccessibilityDbContext, EtiologyView>, IEtiologyViewRepository
    {
        public EtiologyViewRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
