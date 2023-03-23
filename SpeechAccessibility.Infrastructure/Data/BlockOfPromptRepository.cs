using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class BlockOfPromptsRepository : Repository<SpeechAccessibilityDbContext, BlockOfPrompts>, IBlockOfPromptsRepository
    {
        public BlockOfPromptsRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
