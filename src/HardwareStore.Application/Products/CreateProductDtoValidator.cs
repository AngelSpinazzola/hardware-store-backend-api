using FluentValidation;

namespace HardwareStore.Application.Products
{
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del producto es obligatorio")
                .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres")
                .MaximumLength(200).WithMessage("El nombre no puede tener más de 200 caracteres");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("La descripción no puede tener más de 1000 caracteres")
                .When(x => x.Description != null);

            RuleFor(x => x.Price)
                .NotEmpty().WithMessage("El precio es obligatorio")
                .GreaterThan(0.01m).WithMessage("El precio debe ser mayor a 0.01")
                .LessThanOrEqualTo(20000000.00m).WithMessage("El precio no puede ser mayor a 20,000,000");

            RuleFor(x => x.Stock)
                .NotNull().WithMessage("El stock es obligatorio")
                .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("La categoría es obligatoria")
                .GreaterThan(0).WithMessage("Debe seleccionar una categoría válida");

            RuleFor(x => x.Brand)
                .NotEmpty().WithMessage("La marca es obligatoria")
                .MaximumLength(100).WithMessage("La marca no puede tener más de 100 caracteres");

            RuleFor(x => x.Model)
                .MaximumLength(100).WithMessage("El modelo no puede tener más de 100 caracteres")
                .When(x => x.Model != null);

            RuleFor(x => x.Platform)
                .MaximumLength(50).WithMessage("La plataforma no puede tener más de 50 caracteres")
                .When(x => x.Platform != null);
        }
    }
}
