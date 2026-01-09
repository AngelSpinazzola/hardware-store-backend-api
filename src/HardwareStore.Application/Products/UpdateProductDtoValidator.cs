using FluentValidation;

namespace HardwareStore.Application.Products
{
    public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductDtoValidator()
        {
            RuleFor(x => x.Name)
                .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres")
                .MaximumLength(200).WithMessage("El nombre no puede tener más de 200 caracteres")
                .When(x => x.Name != null);

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("La descripción no puede tener más de 1000 caracteres")
                .When(x => x.Description != null);

            RuleFor(x => x.Price)
                .GreaterThan(0.01m).WithMessage("El precio debe ser mayor a 0.01")
                .LessThanOrEqualTo(20000000.00m).WithMessage("El precio no puede ser mayor a 20,000,000")
                .When(x => x.Price.HasValue);

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo")
                .When(x => x.Stock.HasValue);

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Debe seleccionar una categoría válida")
                .When(x => x.CategoryId.HasValue);

            RuleFor(x => x.Brand)
                .MaximumLength(100).WithMessage("La marca no puede tener más de 100 caracteres")
                .When(x => x.Brand != null);

            RuleFor(x => x.Model)
                .MaximumLength(100).WithMessage("El modelo no puede tener más de 100 caracteres")
                .When(x => x.Model != null);

            RuleFor(x => x.Platform)
                .MaximumLength(50).WithMessage("La plataforma no puede tener más de 50 caracteres")
                .When(x => x.Platform != null);
        }
    }
}
