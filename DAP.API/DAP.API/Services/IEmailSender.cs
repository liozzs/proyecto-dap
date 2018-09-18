using System.Threading.Tasks;

namespace DAP.API.Services
{
    public interface IEmailSender
    {
        Task SendEmail(string email, string subject, string message);
    }
}
