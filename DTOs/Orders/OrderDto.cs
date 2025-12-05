namespace EcommerceAPI.DTOs.Orders
{
    public class OrderDto
    {
        public int Id { get; set; }                    
        public int? UserId { get; set; }               
        public int? ShippingAddressId { get; set; }   

        // Información del cliente
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }

        // Dirección de envío (copiada para historial)
        public string ShippingAddressType { get; set; }
        public string ShippingStreet { get; set; }
        public string ShippingNumber { get; set; }
        public string ShippingFloor { get; set; }
        public string ShippingApartment { get; set; }
        public string ShippingTower { get; set; }
        public string ShippingBetweenStreets { get; set; }
        public string ShippingPostalCode { get; set; }
        public string ShippingProvince { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingObservations { get; set; }

        // Persona autorizada (copiada para historial)
        public string AuthorizedPersonFirstName { get; set; }
        public string AuthorizedPersonLastName { get; set; }
        public string AuthorizedPersonPhone { get; set; }
        public string AuthorizedPersonDni { get; set; }

        // Información de la orden (sin cambios)
        public decimal Total { get; set; }
        public string Status { get; set; }
        public string StatusDescription { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentReceiptUrl { get; set; }
        public DateTime? PaymentReceiptUploadedAt { get; set; }
        public DateTime? PaymentSubmittedAt { get; set; }
        public DateTime? PaymentApprovedAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? AdminNotes { get; set; }
        public string? TrackingNumber { get; set; }
        public string? ShippingProvider { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<OrderItemDto> OrderItems { get; set; } = new();
    }
}