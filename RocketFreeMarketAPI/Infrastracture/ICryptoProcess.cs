using RocketFreeMarketAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RocketFreeMarketAPI.Infrastracture
{
    public interface ICryptoProcess
    {
        Secret Encrypt_Aes(string password);
        string Decrypt_Aes(Secret secret);
    }
}
