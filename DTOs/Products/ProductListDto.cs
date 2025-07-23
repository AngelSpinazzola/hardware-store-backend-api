namespace EcommerceAPI.DTOs.Products
{
    public class ProductListDto
    {
        public int Id { get; set; }                    
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; }
        public string Brand { get; set; }             
        public string Model { get; set; }             
        public string MainImageUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
