﻿using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs.Orders
{
    public class CreateOrderDto
    {
        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string CustomerEmail { get; set; }

        [StringLength(20)]
        public string CustomerPhone { get; set; }

        [Required] // Siempre requerido - usuario debe estar registrado y tener dirección
        public int ShippingAddressId { get; set; }

        [Required]
        [StringLength(50)]
        public string ReceiverFirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string ReceiverLastName { get; set; }

        [Required]
        [StringLength(20)]
        public string ReceiverPhone { get; set; }

        [Required]
        [StringLength(20)]
        public string ReceiverDni { get; set; }

        [Required]
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}