using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class ChangePasswordInput
    {
        public string Email { get; set; } = string.Empty;
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
