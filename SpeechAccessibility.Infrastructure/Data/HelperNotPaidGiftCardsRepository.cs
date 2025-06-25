using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class HelperNotPaidGiftCardsRepository : Repository<SpeechAccessibilityDbContext, HelperNotPaidGiftCards>, IHelperNotPaidGiftCardsRepository
    {
        public HelperNotPaidGiftCardsRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
