namespace HardwareStore.Application.Payments
{
    /// <summary>
    /// DTO para recibir notificaciones IPN de MercadoPago via webhook
    /// </summary>
    public class MercadoPagoNotificationDto
    {
        public string Action { get; set; }  // "payment.created", "payment.updated"
        public string Type { get; set; }    // "payment"
        public MercadoPagoNotificationData Data { get; set; }
    }

    public class MercadoPagoNotificationData
    {
        public string Id { get; set; }  // ID del pago en MercadoPago
    }
}
