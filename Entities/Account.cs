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
        public int Status { get; set; }
        public string AccountType { get; set; }

        public static Account CreateAccount(RegisterInput registerInput, Secret secret)
        {
            Account account = new Account()
            {
                Email = registerInput.Email,
                PasswordHash = secret.Cipher,
                PhoneNumber = registerInput.PhoneNumber,
                AesIV = secret.IV,
                AccountType = "Customer"
            };

            return account;
        }

        
    }

    public class RegisterInput
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }


    }

    public class LoginInput
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }



}
