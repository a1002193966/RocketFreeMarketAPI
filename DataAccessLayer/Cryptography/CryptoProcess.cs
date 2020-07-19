﻿using DataAccessLayer.Infrastructure;
using Entities;
using System;
using System.IO;
using System.Security.Cryptography;


namespace DataAccessLayer.Cryptography
{
    public class CryptoProcess : ICryptoProcess
    {
        public Secret Encrypt_Aes(string password)
        {
            Secret secret = new Secret();

            using (Aes aes = Aes.Create())
            {
                secret.Key = aes.Key;
                secret.IV = aes.IV;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(password);
                        }
                        secret.Cipher = ms.ToArray();
                    }
                }
            }
            return secret;
        }

        public byte[] Encrypt_Aes_With_Key_IV(string password, byte[] key, byte[] IV)
        {
            byte[] secret;

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = IV;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(password);
                        }
                        secret = ms.ToArray();
                    }
                }
            }
            return secret;
        }


        public string Decrypt_Aes(Secret secret)
        {
            string password = null;
            using (Aes aes = Aes.Create())
            {
                aes.Key = secret.Key;
                aes.IV = secret.IV;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream(secret.Cipher))
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