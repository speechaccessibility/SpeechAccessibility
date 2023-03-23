using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class BlockOfDigitalCommandPromptsRepository : Repository<SpeechAccessibilityDbContext, BlockOfDigitalCommandPrompts>, IBlockOfDigitalCommandPromptsRepository
    {
        public BlockOfDigitalCommandPromptsRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
