using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class PromptEtiologyRepository : Repository<SpeechAccessibilityDbContext, PromptEtiology>, IPromptEtiologyRepository
    {
        public PromptEtiologyRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
