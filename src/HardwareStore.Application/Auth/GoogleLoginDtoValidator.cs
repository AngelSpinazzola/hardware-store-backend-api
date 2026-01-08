using FluentValidation;

namespace HardwareStore.Application.Auth
{
    public class GoogleLoginDtoValidator : AbstractValidator<GoogleLoginDto>
    {
        public GoogleLoginDtoValidator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty().WithMessage("El token de Google es requerido");
        }
    }
}
