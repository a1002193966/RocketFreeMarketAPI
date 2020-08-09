using DataAccessLayer.Infrastructure;
using DTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Cryptography
{
    public class CryptoProcess : ICryptoProcess
    {
        public async Task<Secret> Encrypt_Aes(string password)
        {
            Secret secret = new Secret();

            using (Aes aes = Aes.Create())
            {
                secret.Key = aes.Key;
                secret.IV = aes.IV;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using MemoryStream ms = new MemoryStream();
                using CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    await sw.WriteAsync(password);
                }
                secret.Cipher = ms.ToArray();
            }
            return secret;
        }

        public async Task<byte[]> Encrypt_Aes_With_Key_IV(string password, byte[] key, byte[] IV)
        {
            byte[] secret;

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = IV;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using MemoryStream ms = new MemoryStream();
                using CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    await sw.WriteAsync(password);
                }
                secret = ms.ToArray();
            }
            return secret;
        }


        public async Task<string> Decrypt_Aes(Secret secret)
        {
            string password = null;
            using (Aes aes = Aes.Create())
            {
                aes.Key = secret.Key;
                aes.IV = secret.IV;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using MemoryStream ms = new MemoryStream(secret.Cipher);
                using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using StreamReader sr = new StreamReader(cs);
                password = await sr.ReadToEndAsync();
            }
            return password;
        }

        public string DecodeHash(string hash)
        {
            dynamic dehash = JsonConvert.DeserializeObject<byte[]>(hash);
            string text = Encoding.ASCII.GetString(dehash);
            return text;
        }

        public string EncodeText(string text)
        {
            byte[] byteString = Encoding.ASCII.GetBytes(text);
            string hash = JsonConvert.SerializeObject(byteString);
            return hash;
        }
    }
}
