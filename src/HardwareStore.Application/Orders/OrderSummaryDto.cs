namespace HardwareStore.Application.Orders
{
    public class OrderSummaryDto
    {
        public int Id { get; set; }                   
        public int? UserId { get; set; }             
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }
        public string StatusDescription { get; set; }
        public bool HasPaymentReceipt { get; set; }
        public string? PaymentReceiptUrl { get; set; }
        public DateTime? PaymentReceiptUploadedAt { get; set; }
        public string? TrackingNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ItemsCount { get; set; }

        // Información básica de envío para el resumen
        public string? ShippingCity { get; set; }
        public string? ShippingProvince { get; set; }
    }
}
