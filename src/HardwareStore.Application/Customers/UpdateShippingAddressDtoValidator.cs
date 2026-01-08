using FluentValidation;

namespace HardwareStore.Application.Customers
{
    public class UpdateShippingAddressDtoValidator : AbstractValidator<UpdateShippingAddressDto>
    {
        public UpdateShippingAddressDtoValidator()
        {
            RuleFor(x => x.AddressType)
                .NotEmpty().WithMessage("El tipo de domicilio es requerido")
                .Must(x => x == "Casa" || x == "Trabajo")
                .WithMessage("El tipo de domicilio debe ser Casa o Trabajo");

            RuleFor(x => x.Street)
                .NotEmpty().WithMessage("La calle es requerida")
                .MinimumLength(2).WithMessage("La calle debe tener al menos 2 caracteres")
                .MaximumLength(100).WithMessage("La calle no puede tener más de 100 caracteres");

            RuleFor(x => x.Number)
                .NotEmpty().WithMessage("La altura es requerida")
                .MinimumLength(1).WithMessage("La altura debe tener al menos 1 carácter")
                .MaximumLength(10).WithMessage("La altura no puede tener más de 10 caracteres")
                .Matches(@"^[\d\-\/\w\s]{1,10}$").WithMessage("Formato de altura inválido");

            RuleFor(x => x.PostalCode)
                .NotEmpty().WithMessage("El código postal es requerido")
                .MinimumLength(4).WithMessage("El código postal debe tener al menos 4 caracteres")
                .MaximumLength(10).WithMessage("El código postal no puede tener más de 10 caracteres")
                .Matches(@"^[\d\s\-]{4,10}$").WithMessage("Formato de código postal inválido");

            RuleFor(x => x.Province)
                .NotEmpty().WithMessage("La provincia es requerida")
                .MinimumLength(2).WithMessage("La provincia debe tener al menos 2 caracteres")
                .MaximumLength(50).WithMessage("La provincia no puede tener más de 50 caracteres");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("La localidad es requerida")
                .MinimumLength(2).WithMessage("La localidad debe tener al menos 2 caracteres")
                .MaximumLength(100).WithMessage("La localidad no puede tener más de 100 caracteres");

            // Campos opcionales
            RuleFor(x => x.Floor)
                .MaximumLength(5).WithMessage("El piso no puede exceder 5 caracteres")
                .Matches(@"^[\w\-\s]{0,5}$").WithMessage("El piso solo puede contener letras, números, guiones y espacios")
                .When(x => !string.IsNullOrEmpty(x.Floor));

            RuleFor(x => x.Apartment)
                .MaximumLength(10).WithMessage("El departamento no puede exceder 10 caracteres")
                .Matches(@"^[\w\-\s]{0,10}$").WithMessage("El departamento solo puede contener letras, números, guiones y espacios")
                .When(x => !string.IsNullOrEmpty(x.Apartment));

            RuleFor(x => x.Tower)
                .MaximumLength(50).WithMessage("La torre no puede exceder 50 caracteres")
                .Matches(@"^[a-zA-ZáéíóúñÁÉÍÓÚÑüÜ\w\s\-\.]{0,50}$").WithMessage("La torre solo puede contener letras, números, espacios, guiones y puntos")
                .When(x => !string.IsNullOrEmpty(x.Tower));

            RuleFor(x => x.BetweenStreets)
                .MaximumLength(200).WithMessage("Las entrecalles no pueden exceder 200 caracteres")
                .Matches(@"^[a-zA-ZáéíóúñÁÉÍÓÚÑüÜ\w\s\-\.\,]{0,200}$").WithMessage("Las entrecalles contienen caracteres inválidos")
                .When(x => !string.IsNullOrEmpty(x.BetweenStreets));

            RuleFor(x => x.Observations)
                .MaximumLength(500).WithMessage("Las observaciones no pueden exceder 500 caracteres")
                .Matches(@"^[a-zA-ZáéíóúñÁÉÍÓÚÑüÜ\w\s\-\.\,\(\)\:]{0,500}$").WithMessage("Las observaciones contienen caracteres inválidos")
                .When(x => !string.IsNullOrEmpty(x.Observations));
        }
    }
}
