using System.ComponentModel.DataAnnotations.Schema;

namespace HardwareStore.Domain.Entities
{
    public class ProductImage
    {
        public int Id { get; set; }                   
        public int ProductId { get; set; }            
        public string ImageUrl { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsMain { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual Product Product { get; set; }
    }
}