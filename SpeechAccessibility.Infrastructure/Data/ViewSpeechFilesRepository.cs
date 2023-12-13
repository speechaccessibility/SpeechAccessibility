using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class ViewSpeechFilesRepository : Repository<SpeechAccessibilityDbContext, ViewSpeechFiles>, IViewSpeechFilesRepository
    {
        public ViewSpeechFilesRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
