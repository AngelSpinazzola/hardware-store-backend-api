using FluentValidation;

namespace HardwareStore.Application.Products
{
    public class CreateProductImageDtoValidator : AbstractValidator<CreateProductImageDto>
    {
        public CreateProductImageDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("El ID del producto es obligatorio")
                .GreaterThan(0).WithMessage("El ID del producto debe ser mayor a 0");

            RuleFor(x => x)
                .Must(x => (x.ImageFiles != null && x.ImageFiles.Length > 0) ||
                          (x.ImageUrls != null && x.ImageUrls.Length > 0))
                .WithMessage("Debe proporcionar al menos una imagen (archivo o URL)");

            RuleFor(x => x.MainImageIndex)
                .GreaterThanOrEqualTo(0).WithMessage("El índice de imagen principal debe ser mayor o igual a 0")
                .When(x => x.MainImageIndex.HasValue);
        }
    }
}
