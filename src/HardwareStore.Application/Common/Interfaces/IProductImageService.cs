using HardwareStore.Application.Products;

namespace HardwareStore.Application.Common.Interfaces
{
    public interface IProductImageService
    {
        Task<IEnumerable<ProductImageDto>> GetProductImagesAsync(int productId);
        Task<IEnumerable<ProductImageDto>> CreateProductImagesAsync(CreateProductImageDto createDto);
        Task<bool> DeleteProductImageAsync(int productId, int imageId);
        Task<bool> SetMainImageAsync(int productId, int imageId);
        Task<bool> UpdateImagesOrderAsync(int productId, UpdateProductImagesOrderDto updateOrderDto);
        Task<ProductImageDto?> GetProductImageAsync(int productId, int imageId);
    }
}
