using System.ComponentModel.DataAnnotations;

namespace Evently.Api.DTOs.Auth
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; }
            = string.Empty;
    }
}