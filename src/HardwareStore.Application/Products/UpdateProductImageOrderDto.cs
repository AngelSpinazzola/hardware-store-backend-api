using HardwareStore.Domain.Enums;

namespace HardwareStore.Application.Products
{
    public class UpdateProductImageOrderDto
    {
        public int ImageId { get; set; }
        public int DisplayOrder { get; set; }
    }
}
