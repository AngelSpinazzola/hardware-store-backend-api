using HardwareStore.Domain.Enums;
﻿using HardwareStore.Domain.Entities;

namespace HardwareStore.Application.Products
{
    public class ProductListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string? Platform { get; set; }
        public string MainImageUrl { get; set; }
        public ProductStatus Status { get; set; }
    }
}
