namespace HardwareStore.Application.Orders
{
    public class CreateOrderDto
    {
        public string CustomerName { get; set; }
        public int ShippingAddressId { get; set; }
        public string ReceiverFirstName { get; set; }
        public string ReceiverLastName { get; set; }
        public string ReceiverPhone { get; set; }
        public string ReceiverDni { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
        public string PaymentMethod { get; set; } = "bank_transfer";
    }
}