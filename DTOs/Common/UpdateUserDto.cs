using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs.Common
{
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
        public string LastName { get; set; }
    }
}
