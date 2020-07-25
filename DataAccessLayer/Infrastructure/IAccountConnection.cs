using DTO;
using Entities;

namespace DataAccessLayer.Infrastructure
{
    public interface IAccountConnection
    {     
        bool Register(RegisterInput registerInput);
        bool Login(LoginInput loginInput);
        Account GetAccountInfo(string email);
    }
}
