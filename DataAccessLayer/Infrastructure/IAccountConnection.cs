using DTO;
using Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Infrastructure
{
    public interface IAccountConnection
    {
        Task<int> Register(RegisterInput registerInput);
        Task<int> Login(LoginInput loginInput);
        Task<Account> GetAccountInfo(string email);
        Task<bool> ActivateAccount(string email, string token);
    }
}
