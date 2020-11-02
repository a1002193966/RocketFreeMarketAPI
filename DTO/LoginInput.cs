using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class LoginInput
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        public string ReCaptchaToken { get; set; }
    }
}
