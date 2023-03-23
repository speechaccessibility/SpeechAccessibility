using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class BlockOfDigitalCommandRepository : Repository<SpeechAccessibilityDbContext, BlockOfDigitalCommand>, IBlockOfDigitalCommandRepository
    {
        public BlockOfDigitalCommandRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
