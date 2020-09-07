namespace DTO
{
    public class SmtpPackage
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public Secret UsernamePackage { get; set; }
        public Secret PasswordPackage { get; set; }
    }
}
