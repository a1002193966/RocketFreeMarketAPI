
namespace DTO
{
    public class Secret
    {
        public byte[] Cipher { get; set; }
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }
    }
}
