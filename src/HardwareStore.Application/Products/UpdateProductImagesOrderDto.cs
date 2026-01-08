using HardwareStore.Domain.Enums;

namespace HardwareStore.Application.Products
{
    public class UpdateProductImagesOrderDto
    {
        public List<UpdateProductImageOrderDto> Images { get; set; } = new();
    }
}
