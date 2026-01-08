using FluentValidation;

namespace HardwareStore.Application.Auth
{
    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("El token es requerido")
                .MaximumLength(255).WithMessage("El token no puede tener más de 255 caracteres");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("La nueva contraseña es requerida")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres")
                .MaximumLength(100).WithMessage("La contraseña no puede tener más de 100 caracteres");
        }
    }
}
