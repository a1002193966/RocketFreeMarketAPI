using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO
{
    public class ChangePasswordInput
    {
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(30, MinimumLength = 6)]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 6)]
        public string NewPassword { get; set; }
    }
}
