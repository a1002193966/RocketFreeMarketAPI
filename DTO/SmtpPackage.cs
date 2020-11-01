using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class SmtpPackage
    {
        [Required]
        public string Host { get; set; }
        [Required]
        public int Port { get; set; }
        [Required]
        public Secret UsernamePackage { get; set; }
        [Required]
        public Secret PasswordPackage { get; set; }
    }

    public class SmtpPackageSerialized
    {
        [Required]
        public string Host { get; set; }
        [Required]
        public int Port { get; set; }
        [Required]
        public SecretSerialized UsernamePackage { get; set; }
        [Required]
        public SecretSerialized PasswordPackage { get; set; }
    }
}
