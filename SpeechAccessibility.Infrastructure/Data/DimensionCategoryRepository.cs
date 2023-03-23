using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class DimensionCategoryRepository : Repository<SpeechAccessibilityDbContext, DimensionCategory>, IDimensionCategoryRepository
    {
        public DimensionCategoryRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
