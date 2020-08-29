using DTO;
using System.Threading.Tasks;

namespace DataAccessLayer.Infrastructure
{
    public interface ICryptoProcess
    {
        Task<Secret> Encrypt_Aes(string password);
        Task<byte[]> Encrypt_Aes_With_Key_IV(string password, byte[] key, byte[] IV);
        Task<string> Decrypt_Aes(Secret secret);
        string DecodeHash(string hash);
        string EncodeText(string text);
        bool ValidateVerificationToken(string token);
    }
}
