﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using RocketFreeMarketAPI.Models;

namespace RocketFreeMarketAPI.Crypto
{
    public static class CryptoProcess
    {
        public static Secret Encrypt_Aes(string password)
        {
            Secret secret = new Secret();

            using (Aes aes = Aes.Create())
            {
                secret.Key = aes.Key;
                secret.IV = aes.IV;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using(CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(password);
                        }
                        secret.PasswordHash = ms.ToArray();
                    }
                }
            }
            return secret;
        }


        public static string Decrypt_Aes(Secret secret)
        {
            string password = null;
            using (Aes aes = Aes.Create())
            {
                aes.Key = secret.Key;
                aes.IV = secret.IV;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream(secret.PasswordHash))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            password = sr.ReadToEnd();
                        }
                    }
                }
            }
            return password;
        }
    }
}
