using System.Threading.Tasks;

namespace DataAccessLayer.Infrastructure
{
    public interface IEmailSender
    {
        Task<bool> ExecuteSender(string email, string tokenType);
    }
}
