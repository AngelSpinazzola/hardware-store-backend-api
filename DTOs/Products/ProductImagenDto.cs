namespace EcommerceAPI.DTOs.Products
{
    public class ProductImageDto
    {
        public int Id { get; set; }                   
        public int ProductId { get; set; }             
        public string ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsMain { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
