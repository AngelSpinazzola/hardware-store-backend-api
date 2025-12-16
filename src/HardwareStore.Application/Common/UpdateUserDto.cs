using System.ComponentModel.DataAnnotations;

namespace HardwareStore.Application.Common
{
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúñÁÉÍÓÚÑüÜ\s\-\.]+$", ErrorMessage = "El nombre solo puede contener letras, espacios, guiones y puntos")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúñÁÉÍÓÚÑüÜ\s\-\.]+$", ErrorMessage = "El apellido solo puede contener letras, espacios, guiones y puntos")]
        public string LastName { get; set; }
    }
}
