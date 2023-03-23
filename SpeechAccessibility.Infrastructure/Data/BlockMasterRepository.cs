using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class BlockMasterRepository : Repository<SpeechAccessibilityDbContext, BlockMaster>, IBlockMasterRepository
    {
        public BlockMasterRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
