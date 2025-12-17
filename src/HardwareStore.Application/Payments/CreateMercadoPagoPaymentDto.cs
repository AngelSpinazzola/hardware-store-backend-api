namespace HardwareStore.Application.Payments
{
    public class CreateMercadoPagoPaymentDto
    {
        public int OrderId { get; set; }
        public string BackUrl { get; set; } 
    }
}
