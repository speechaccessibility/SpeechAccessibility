using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class GiftCardAmountRepository : Repository<SpeechAccessibilityDbContext, GiftCardAmount>, IGiftCardAmountRepository
    {
        public GiftCardAmountRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
