using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class Secret
    {
        [Required]
        public byte[] Cipher { get; set; }
        [Required]
        public byte[] Key { get; set; }
        [Required]
        public byte[] IV { get; set; }
    }

    public class SecretSerialized
    {
        [Required]
        public string Cipher { get; set; }
        [Required]
        public string Key { get; set; }
        [Required]
        public string IV { get; set; }
    }
}
