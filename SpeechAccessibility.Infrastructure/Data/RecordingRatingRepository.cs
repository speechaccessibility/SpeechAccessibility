using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class RecordingRatingRepository : Repository<SpeechAccessibilityDbContext,RecordingRating>, IRecordingRatingRepository
    {
        public RecordingRatingRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
