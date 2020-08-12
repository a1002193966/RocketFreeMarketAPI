using System;

namespace DTO
{
    public class AccountDTO
    {
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] AesIV { get; set; }
        public string AccountType { get; set; }

        public AccountDTO()
        {
        }

        public AccountDTO(RegisterInput registerInput, Secret secret)
        {
            this.Email = registerInput.Email;
            this.NormalizedEmail = registerInput.Email.ToUpper();
            this.PasswordHash = secret.Cipher;
            this.PhoneNumber = registerInput.PhoneNumber;
            this.AesIV = secret.IV;
            this.AccountType = "Customer";
        }
    }
}
