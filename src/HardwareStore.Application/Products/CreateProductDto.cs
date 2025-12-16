using HardwareStore.Domain.Enums;
using Microsoft.AspNetCore.Http;
﻿using System.ComponentModel.DataAnnotations;

namespace HardwareStore.Application.Products
{
    public class CreateProductDto
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, 20000000.00)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; }

        [Required]                                  
        [StringLength(100)]
        public string Brand { get; set; }

        [StringLength(100)]                            
        public string Model { get; set; }

        [StringLength(50)]
        public string? Platform { get; set; }

        public IFormFile[]? ImageFiles { get; set; }
        public string[]? ImageUrls { get; set; }
    }
}
