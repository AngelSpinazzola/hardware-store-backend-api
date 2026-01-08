using FluentValidation;

namespace HardwareStore.Application.Products
{
    public class UpdateProductImagesOrderDtoValidator : AbstractValidator<UpdateProductImagesOrderDto>
    {
        public UpdateProductImagesOrderDtoValidator()
        {
            RuleFor(x => x.Images)
                .NotEmpty().WithMessage("La lista de imágenes es obligatoria")
                .Must(images => images != null && images.Count > 0)
                .WithMessage("Debe incluir al menos una imagen");

            RuleForEach(x => x.Images).SetValidator(new UpdateProductImageOrderDtoValidator());
        }
    }
}
