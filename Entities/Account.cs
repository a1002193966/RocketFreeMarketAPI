﻿using System;

namespace Entities
{
    public class Account
    {
        public string AccountID { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] AesKey { get; set; }
        public byte[] AesIV { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public bool EmailVerificationStatus { get; set; }
        public bool PhoneVerificationStatus { get; set; }
        public int AccountStatus { get; set; }
        public string AccountType { get; set; }
    }
}
