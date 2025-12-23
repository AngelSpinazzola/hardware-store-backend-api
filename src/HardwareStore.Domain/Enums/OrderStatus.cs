namespace HardwareStore.Domain.Enums
{
    public static class OrderStatus
    {
        public const string PendingPayment = "pending_payment";
        public const string PaymentSubmitted = "payment_submitted";
        public const string PaymentApproved = "payment_approved";
        public const string PaymentRejected = "payment_rejected";
        public const string Shipped = "shipped";
        public const string Delivered = "delivered";
        public const string Cancelled = "cancelled";

        // Payment Methods
        public const string PaymentMethodBankTransfer = "bank_transfer";
        public const string PaymentMethodMercadoPago = "mercadopago";

        public static bool IsValidStatus(string status)
        {
            return status switch
            {
                PendingPayment or PaymentSubmitted or PaymentApproved or
                PaymentRejected or Shipped or Delivered or Cancelled => true,
                _ => false
            };
        }

        public static string GetStatusDescription(string status)
        {
            return status switch
            {
                PendingPayment => "Esperando comprobante de pago",
                PaymentSubmitted => "Comprobante en revisión",
                PaymentApproved => "Pago aprobado - Preparando envío",
                PaymentRejected => "Comprobante rechazado",
                Shipped => "Enviado",
                Delivered => "Entregado",
                Cancelled => "Cancelado",
                _ => "Estado desconocido"
            };
        }

        public static bool IsValidPaymentMethod(string method)
        {
            return method is PaymentMethodBankTransfer or PaymentMethodMercadoPago;
        }
    }
}