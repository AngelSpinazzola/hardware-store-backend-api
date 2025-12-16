namespace HardwareStore.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }                   
        public int OrderId { get; set; }             
        public int ProductId { get; set; }             
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }

        // Datos del producto (historial)
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
        public string ProductBrand { get; set; }       
        public string ProductModel { get; set; }      

        // Navigation properties
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}
