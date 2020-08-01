using System;

namespace Entities
{
    public class Account
    {
        public int AccountID { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] AesKey { get; set; }
        public byte[] AesIV { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public int EmailVerificationStatus { get; set; }
        public int PhoneVerificationStatus { get; set; }
        public int AccountStatus { get; set; }
        public string AccountType { get; set; }
    }
}
