using HardwareStore.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace HardwareStore.Application.Products
{
    public class CreateProductImageDto
    {
        public int ProductId { get; set; }
        public IFormFile[]? ImageFiles { get; set; }
        public string[]? ImageUrls { get; set; }
        public int? MainImageIndex { get; set; }
    }
}
