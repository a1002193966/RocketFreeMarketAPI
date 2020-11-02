
namespace DTO
{
    public class Secret
    {
        public byte[] Cipher { get; set; }
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }
    }

    public class SecretSerialized
    {
        public string Cipher { get; set; }
        public string Key { get; set; }
        public string IV { get; set; }
    }
}
