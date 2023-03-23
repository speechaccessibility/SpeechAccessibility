using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class SubCategoryRepository : Repository<SpeechAccessibilityDbContext, SubCategory>, ISubCategoryRepository
    {
        public SubCategoryRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
