using DTO;
using System.Threading.Tasks;

namespace DataAccessLayer.Infrastructure
{
    public interface IAccountConnection
    {
        Task<ERegisterStatus> Register(RegisterInput registerInput);
        Task<ELoginStatus> Login(LoginInput loginInput);
        Task<EEmailVerifyStatus> ActivateAccount(string email, string token);
    }
}
