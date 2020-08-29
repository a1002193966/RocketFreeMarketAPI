using DTO;
using System.Threading.Tasks;

namespace DataAccessLayer.Infrastructure
{
    public interface IAccountConnection
    {
        Task<EEmailRegister> Register(RegisterInput registerInput);
        Task<EAccountStatus> Login(LoginInput loginInput);
        Task<EActivateAccount> ActivateAccount(string email, string token);
    }
}
