using DTO;
using System.Threading.Tasks;

namespace DataAccessLayer.Infrastructure
{
    public interface ILoginToken
    {
        Task<string> GenerateToken(LoginInput loginInput);
    }
}
