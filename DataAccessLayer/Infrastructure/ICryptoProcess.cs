using Entities;

namespace DataAccessLayer.Infrastructure
{
    public interface ICryptoProcess
    {
        Secret Encrypt_Aes(string password);
        byte[] Encrypt_Aes_With_Key_IV(string password, byte[] key, byte[] IV);
        string Decrypt_Aes(Secret secret);
    }
}
