namespace HardwareStore.Application.Payments
{
    public interface IMercadoPagoService
    {
        Task<MercadoPagoPaymentResponseDto> CreatePaymentPreferenceAsync(int orderId, string backUrl);
        Task<MercadoPagoPaymentInfo> GetPaymentInfoAsync(string paymentId);
        Task ProcessWebhookNotificationAsync(string paymentId);
    }

    public class MercadoPagoPaymentInfo
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string StatusDetail { get; set; }
        public decimal TransactionAmount { get; set; }
        public string PaymentTypeId { get; set; }
        public string ExternalReference { get; set; }
    }
}
