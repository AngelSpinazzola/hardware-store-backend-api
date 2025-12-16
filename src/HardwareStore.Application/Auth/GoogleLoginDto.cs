using System.ComponentModel.DataAnnotations;

namespace HardwareStore.Application.Auth
{
    public class GoogleLoginDto
    {
        [Required(ErrorMessage = "El token de Google es requerido")]
        public string IdToken { get; set; } = string.Empty;
    }
}
