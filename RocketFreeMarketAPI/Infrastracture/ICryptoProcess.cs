using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RocketFreeMarketAPI.Infrastracture
{
    public interface ICryptoProcess
    {
        Secret Encrypt_Aes(string password);
        byte[] Encrypt_Aes_With_Key_IV(string password, byte[] key, byte[] IV);
        string Decrypt_Aes(Secret secret);
    }
}
