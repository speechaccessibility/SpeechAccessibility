using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class RecordingStatusRepository : Repository<SpeechAccessibilityDbContext,RecordingStatus>, IRecordingStatusRepository
    {
        public RecordingStatusRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
