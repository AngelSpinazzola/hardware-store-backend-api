using System.ComponentModel.DataAnnotations;

namespace HardwareStore.Application.Auth
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
    }
}
