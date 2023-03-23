using System.Threading.Tasks;

namespace SpeechAccessibility.Services
{
    public interface IMailService
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}