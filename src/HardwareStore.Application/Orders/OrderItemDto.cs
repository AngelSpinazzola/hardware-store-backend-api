namespace HardwareStore.Application.Orders
{
    public class OrderItemDto
    {
        public int Id { get; set; }                  
        public int ProductId { get; set; }             
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }

        // Brand y Model para historial
        public string ProductBrand { get; set; }
        public string ProductModel { get; set; }
    }
}