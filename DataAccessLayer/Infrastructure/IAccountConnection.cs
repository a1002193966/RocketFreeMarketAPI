using DTO;
using System.Threading.Tasks;

namespace DataAccessLayer.Infrastructure
{
    public interface IAccountConnection
    {
        Task<int> Register(RegisterInput registerInput);
        Task<int> Login(LoginInput loginInput);
        Task<int> ActivateAccount(string email, string token);
    }
}
