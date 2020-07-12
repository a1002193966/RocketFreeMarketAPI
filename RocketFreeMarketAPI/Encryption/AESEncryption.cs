using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace RocketFreeMarketAPI.Encryption
{
    public class AESEncryption
    {
        public (byte[], byte[]) CreateAES(string secret)
        {
   
            using (Aes myAes = Aes.Create())
            {

                // AES provide Error handle using, no need try, catch !!!
            
                // Encrypt the string to an array of bytes.
                byte[] encrypted = EncryptStringToBytes_Aes(secret, myAes.Key, myAes.IV);

     
                //Display the original data and the decrypted data.
                Console.WriteLine("Original:   {0}", secret);
                return (myAes.Key, myAes.IV);
            }
         
        }
        private byte[] EncryptStringToBytes_Aes(string secret, byte[] AESKey, byte[] AESIV)
        {
            // Check arguments.
            if (secret == null || secret.Length <= 0)
                throw new ArgumentNullException("secret");
            if (AESKey == null || AESKey.Length <= 0)
                throw new ArgumentNullException("AESKey");
            if (AESIV == null || AESIV.Length <= 0)
                throw new ArgumentNullException("AESIV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = AESKey;
                aesAlg.IV = AESIV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(secret);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }// end Encryption

        private string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }//end decryption
    }
}
