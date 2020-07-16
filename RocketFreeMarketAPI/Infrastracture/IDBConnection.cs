using RocketFreeMarketAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RocketFreeMarketAPI.Infrastracture
{
    public interface IDBConnection
    {
        bool Register(RegisterInput registerInput);
        bool Login(LoginInput loginInput);
        Account GetAccountInfo(string email);
    }
}
