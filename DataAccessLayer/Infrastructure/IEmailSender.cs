

namespace DataAccessLayer.Infrastructure
{
    public interface IEmailSender
    {
        void ExecuteSender(string email);
    }
}
