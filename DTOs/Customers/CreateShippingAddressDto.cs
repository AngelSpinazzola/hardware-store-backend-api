using EcommerceAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs.Customers
{
    public class CreateShippingAddressDto
    {
        [Required(ErrorMessage = "El tipo de domicilio es requerido")]
        [RegularExpression(@"^(Casa|Trabajo)$", ErrorMessage = "El tipo de domicilio debe ser Casa o Trabajo")]
        public string AddressType { get; set; }

        [Required(ErrorMessage = "La calle es requerida")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "La calle debe tener entre 2 y 100 caracteres")]
        public string Street { get; set; }

        [Required(ErrorMessage = "La altura es requerida")]
        [StringLength(10, MinimumLength = 1, ErrorMessage = "La altura debe tener entre 1 y 10 caracteres")]
        [RegularExpression(@"^[\d\-\/\w\s]{1,10}$", ErrorMessage = "Formato de altura inválido")]
        public string Number { get; set; }

        [Required(ErrorMessage = "El código postal es requerido")]
        [StringLength(10, MinimumLength = 4, ErrorMessage = "El código postal debe tener entre 4 y 10 caracteres")]
        [RegularExpression(@"^[\d\s\-]{4,10}$", ErrorMessage = "Formato de código postal inválido")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "La provincia es requerida")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "La provincia debe tener entre 2 y 50 caracteres")]
        public string Province { get; set; }

        [Required(ErrorMessage = "La localidad es requerida")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "La localidad debe tener entre 2 y 100 caracteres")]
        public string City { get; set; }
        
        // Campos opcionales

        [StringLength(5, ErrorMessage = "El piso no puede exceder 5 caracteres")]
        [RegularExpression(@"^[\w\-\s]{0,5}$", ErrorMessage = "El piso solo puede contener letras, números, guiones y espacios")]
        public string? Floor { get; set; }

        [StringLength(10, ErrorMessage = "El departamento no puede exceder 10 caracteres")]
        [RegularExpression(@"^[\w\-\s]{0,10}$", ErrorMessage = "El departamento solo puede contener letras, números, guiones y espacios")]
        public string? Apartment { get; set; }

        [StringLength(50, ErrorMessage = "La torre no puede exceder 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúñÁÉÍÓÚÑüÜ\w\s\-\.]{0,50}$", ErrorMessage = "La torre solo puede contener letras, números, espacios, guiones y puntos")]
        public string? Tower { get; set; }

        [StringLength(200, ErrorMessage = "Las entrecalles no pueden exceder 200 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúñÁÉÍÓÚÑüÜ\w\s\-\.\,]{0,200}$", ErrorMessage = "Las entrecalles contienen caracteres inválidos")]
        public string? BetweenStreets { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúñÁÉÍÓÚÑüÜ\w\s\-\.\,\(\)\:]{0,500}$", ErrorMessage = "Las observaciones contienen caracteres inválidos")]
        public string? Observations { get; set; }
    }
}