using FluentValidation;

namespace HardwareStore.Application.Common
{
    public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres")
                .MaximumLength(50).WithMessage("El nombre no puede tener más de 50 caracteres")
                .Matches(@"^[a-zA-ZáéíóúñÁÉÍÓÚÑüÜ\s\-\.]+$")
                .WithMessage("El nombre solo puede contener letras, espacios, guiones y puntos");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("El apellido es requerido")
                .MinimumLength(2).WithMessage("El apellido debe tener al menos 2 caracteres")
                .MaximumLength(50).WithMessage("El apellido no puede tener más de 50 caracteres")
                .Matches(@"^[a-zA-ZáéíóúñÁÉÍÓÚÑüÜ\s\-\.]+$")
                .WithMessage("El apellido solo puede contener letras, espacios, guiones y puntos");
        }
    }
}
