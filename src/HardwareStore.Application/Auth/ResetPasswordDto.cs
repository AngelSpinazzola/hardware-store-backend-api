using System.ComponentModel.DataAnnotations;

namespace HardwareStore.Application.Auth
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "El token es requerido")]
        [StringLength(255)]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
