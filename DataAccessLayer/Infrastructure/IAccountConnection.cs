using DTO;
using System.Threading.Tasks;

namespace DataAccessLayer.Infrastructure
{
    public interface IAccountConnection
    {
        Task<EStatus> Register(RegisterInput registerInput);
        Task<ELoginStatus> Login(LoginInput loginInput);
        Task<EStatus> ActivateAccount(string email, string token);
        Task<EStatus> ChangePassword(ChangePasswordInput changePasswordInput);
        Task SendResetLink(string email);
        Task<EStatus> ResetPassword(ResetPasswordInput resetDTO);
    }
}
