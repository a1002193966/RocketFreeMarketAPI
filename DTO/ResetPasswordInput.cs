using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class ResetPasswordInput
    {
        public string EncryptedEmail { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
    }
}
