using DTO;
using System.Threading.Tasks;

namespace DataAccessLayer.Infrastructure
{
    public interface IAccountConnection
    {
        Task<EEmailStatus> Register(RegisterInput registerInput);
        Task<EAccountStatus> Login(LoginInput loginInput);
        Task<int> ActivateAccount(string email, string token);
    }
}
