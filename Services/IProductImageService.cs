using EcommerceAPI.DTOs.Products;

namespace EcommerceAPI.Services
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
