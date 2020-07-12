using System;

namespace RocketFreeMarketAPI.Models
{
    public class Account
    {
        public int AccountID { get; set; }
        public int PhoneNumber { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public byte[] AESKey { get; set; }
        public byte[] AESIV { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public int Status { get; set; }
        public string AccountType { get; set; }
    }
}
