using System.ComponentModel.DataAnnotations;

namespace Evently.Api.DTOs.Auth
{
    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; }
            = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage =
                "La contraseña debe contener una mayúscula, una minúscula y un número."
        )]
        public string Password { get; set; }
            = string.Empty;

        [Required]
        [Compare(
            nameof(Password),
            ErrorMessage =
                "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; }
            = string.Empty;
    }
}