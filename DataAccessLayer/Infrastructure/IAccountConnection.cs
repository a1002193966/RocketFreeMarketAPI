using Entities;


namespace DataAccessLayer.Infrastructure
{
    public interface IDatabaseConnection
    {     
        bool Register(RegisterInput registerInput);
        bool Login(LoginInput loginInput);
        Account GetAccountInfo(string email);     
    }
}
