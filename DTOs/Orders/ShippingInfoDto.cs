using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs.Orders
{
    public class ShippingInfoDto
    {
        [Required]
        public string TrackingNumber { get; set; }

        [Required]
        public string ShippingProvider { get; set; }

        public string? AdminNotes { get; set; }
    }
}
