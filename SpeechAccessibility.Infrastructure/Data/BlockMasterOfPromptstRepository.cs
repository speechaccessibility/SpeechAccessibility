using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class BlockMasterOfPromptsRepository : Repository<SpeechAccessibilityDbContext, BlockMasterOfPrompts>, IBlockMasterOfPromptsRepository
    {
        public BlockMasterOfPromptsRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
