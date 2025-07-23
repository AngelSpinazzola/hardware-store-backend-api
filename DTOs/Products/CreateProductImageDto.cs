using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs.Products
{
    public class CreateProductImageDto
    {
        [Required]
        public int ProductId { get; set; }             

        public IFormFile[]? ImageFiles { get; set; }
        public string[]? ImageUrls { get; set; }
        public int? MainImageIndex { get; set; }
    }
}
