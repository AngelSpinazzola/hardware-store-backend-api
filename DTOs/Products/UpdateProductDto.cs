using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs.Products
{
    public class UpdateProductDto
    {
        [StringLength(200, MinimumLength = 2)]
        public string? Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(0.01, 20000000.00)]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue)]
        public int? Stock { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(100)]                      
        public string? Brand { get; set; }

        [StringLength(100)]                     
        public string? Model { get; set; }

        [StringLength(50)]
        public string? Platform { get; set; }

        public IFormFile[]? ImageFiles { get; set; }
        public string[]? ImageUrls { get; set; }
        public bool? IsActive { get; set; }
    }
}
