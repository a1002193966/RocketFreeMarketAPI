using DTO;
using Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.Infrastructure
{
    public interface IAccountConnection
    {
        bool Register(RegisterInput registerInput);
        int Login(LoginInput loginInput);
        Account GetAccountInfo(string email);
        bool ActivateAccount(string email, string token);
    }
}
