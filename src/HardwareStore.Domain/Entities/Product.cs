using HardwareStore.Domain.Enums;

namespace HardwareStore.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string MainImageUrl { get; set; }
        public string? Platform { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        // Relaciones
        public virtual Category Category { get; set; }
        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}