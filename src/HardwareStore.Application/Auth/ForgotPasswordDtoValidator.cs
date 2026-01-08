using FluentValidation;

namespace HardwareStore.Application.Auth
{
    public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
    {
        public ForgotPasswordDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("Email inválido")
                .MaximumLength(100).WithMessage("El email no puede tener más de 100 caracteres");
        }
    }
}
