using FluentValidation;
using HardwareStore.Domain.Enums;

namespace HardwareStore.Application.Orders
{
    public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
    {
        public UpdateOrderStatusDtoValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("El estado es obligatorio")
                .Must(OrderStatus.IsValidStatus)
                .WithMessage($"El estado debe ser uno de los siguientes: {OrderStatus.PendingPayment}, {OrderStatus.PaymentSubmitted}, {OrderStatus.PaymentApproved}, {OrderStatus.PaymentRejected}, {OrderStatus.Shipped}, {OrderStatus.Delivered}, {OrderStatus.Cancelled}");

            RuleFor(x => x.AdminNotes)
                .MaximumLength(1000).WithMessage("Las notas del administrador no pueden exceder 1000 caracteres")
                .When(x => !string.IsNullOrEmpty(x.AdminNotes));
        }
    }
}
