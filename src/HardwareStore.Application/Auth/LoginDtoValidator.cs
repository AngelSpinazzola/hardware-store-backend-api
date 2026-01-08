using FluentValidation;

namespace HardwareStore.Application.Auth
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es obligatorio")
                .EmailAddress().WithMessage("Debe ingresar un email válido")
                .MaximumLength(100).WithMessage("El email no puede tener más de 100 caracteres");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria");
        }
    }
}
