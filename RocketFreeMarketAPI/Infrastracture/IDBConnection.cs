using Entities;

namespace RocketFreeMarketAPI.Infrastracture
{
    public interface IDBConnection
    {
        bool Register(RegisterInput registerInput);
        bool Login(LoginInput loginInput);
        Account GetAccountInfo(string email);
    }
}
