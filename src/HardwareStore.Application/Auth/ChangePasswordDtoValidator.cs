using FluentValidation;

namespace HardwareStore.Application.Auth
{
    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("La contraseña actual es requerida");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("La nueva contraseña es requerida")
                .MinimumLength(8).WithMessage("La nueva contraseña debe tener al menos 8 caracteres")
                .MaximumLength(100).WithMessage("La nueva contraseña no puede tener más de 100 caracteres");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("La confirmación de contraseña es requerida")
                .Equal(x => x.NewPassword).WithMessage("Las contraseñas no coinciden");
        }
    }
}
