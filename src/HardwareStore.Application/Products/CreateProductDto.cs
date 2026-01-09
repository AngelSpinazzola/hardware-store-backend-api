using HardwareStore.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace HardwareStore.Application.Products
{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string? Platform { get; set; }
        public IFormFile[]? ImageFiles { get; set; }
        public string[]? ImageUrls { get; set; }
    }
}
