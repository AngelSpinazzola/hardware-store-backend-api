using FluentValidation;

namespace HardwareStore.Application.Products
{
    public class UpdateProductImageOrderDtoValidator : AbstractValidator<UpdateProductImageOrderDto>
    {
        public UpdateProductImageOrderDtoValidator()
        {
            RuleFor(x => x.ImageId)
                .NotEmpty().WithMessage("El ID de la imagen es obligatorio")
                .GreaterThan(0).WithMessage("El ID de la imagen debe ser mayor a 0");

            RuleFor(x => x.DisplayOrder)
                .NotNull().WithMessage("El orden de visualización es obligatorio")
                .GreaterThanOrEqualTo(0).WithMessage("El orden de visualización debe ser mayor o igual a 0");
        }
    }
}
