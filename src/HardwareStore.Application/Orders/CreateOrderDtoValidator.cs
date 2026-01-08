using FluentValidation;

namespace HardwareStore.Application.Orders
{
    public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderDtoValidator()
        {
            RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage("El nombre del cliente es obligatorio")
                .MaximumLength(100).WithMessage("El nombre no puede tener más de 100 caracteres")
                .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres");

            RuleFor(x => x.ShippingAddressId)
                .GreaterThan(0).WithMessage("Debe seleccionar una dirección de envío válida");

            RuleFor(x => x.ReceiverFirstName)
                .NotEmpty().WithMessage("El nombre del receptor es obligatorio")
                .MaximumLength(50).WithMessage("El nombre del receptor no puede tener más de 50 caracteres");

            RuleFor(x => x.ReceiverLastName)
                .NotEmpty().WithMessage("El apellido del receptor es obligatorio")
                .MaximumLength(50).WithMessage("El apellido del receptor no puede tener más de 50 caracteres");

            RuleFor(x => x.ReceiverPhone)
                .NotEmpty().WithMessage("El teléfono del receptor es obligatorio")
                .MaximumLength(20).WithMessage("El teléfono no puede tener más de 20 caracteres")
                .Matches(@"^[\d\s\+\-\(\)]+$").WithMessage("El teléfono solo puede contener números, espacios y símbolos + - ( )");

            RuleFor(x => x.ReceiverDni)
                .NotEmpty().WithMessage("El DNI del receptor es obligatorio")
                .MaximumLength(20).WithMessage("El DNI no puede tener más de 20 caracteres")
                .Matches(@"^[0-9]{7,8}$").WithMessage("El DNI debe contener entre 7 y 8 dígitos");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Debe incluir al menos un producto en la orden")
                .Must(items => items != null && items.Count > 0)
                    .WithMessage("La orden debe contener al menos un producto");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId)
                    .GreaterThan(0).WithMessage("El ID del producto debe ser válido");

                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0")
                    .LessThanOrEqualTo(100).WithMessage("No puede ordenar más de 100 unidades del mismo producto");
            });

            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("El método de pago es obligatorio")
                .MaximumLength(20).WithMessage("El método de pago no puede tener más de 20 caracteres")
                .Must(pm => pm == "mercadopago" || pm == "bank_transfer")
                    .WithMessage("El método de pago debe ser 'mercadopago' o 'bank_transfer'");
        }
    }
}
