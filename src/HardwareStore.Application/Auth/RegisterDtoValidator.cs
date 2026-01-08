using FluentValidation;

namespace HardwareStore.Application.Auth
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es obligatorio")
                .EmailAddress().WithMessage("Debe ingresar un email válido")
                .MaximumLength(100).WithMessage("El email no puede tener más de 100 caracteres");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres")
                .MaximumLength(100).WithMessage("La contraseña no puede tener más de 100 caracteres");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("El nombre es obligatorio")
                .MaximumLength(50).WithMessage("El nombre no puede tener más de 50 caracteres");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("El apellido es obligatorio")
                .MaximumLength(50).WithMessage("El apellido no puede tener más de 50 caracteres");
        }
    }
}
