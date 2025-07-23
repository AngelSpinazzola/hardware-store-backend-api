using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs.Products
{
    public class SetMainImageDto
    {
        [Required]
        public int ImageId { get; set; }             
    }
}
