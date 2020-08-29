using DTO;

namespace DataAccessLayer.Infrastructure
{
    public interface ILoginToken
    {
        string GenerateToken(LoginInput loginInput);
    }
}
