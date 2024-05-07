using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class NumberOfRecordingByEtiologyRepository : Repository<SpeechAccessibilityDbContext, NumberOfRecordingByEtiology>, INumberOfRecordingByEtiologyRepository
    {
        public NumberOfRecordingByEtiologyRepository(SpeechAccessibilityDbContext speechAccessibilityDbContext) : base(speechAccessibilityDbContext)
        {
        }
    }
}
