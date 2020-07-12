using RocketFreeMarketAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RocketFreeMarketAPI.Infrastracture
{
    public interface IDBConnection
    {
        Task<List<Account>> ExcuteCommand(string cmd);
        bool Register(string email, string pwd, int phoneNumber);
    }
}
