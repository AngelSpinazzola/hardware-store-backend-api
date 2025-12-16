using HardwareStore.Domain.Entities;

namespace HardwareStore.Domain.Interfaces
{
    public interface IProductImageRepository
    {
        Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId);
        Task<ProductImage?> GetByIdAsync(int id);
        Task<ProductImage> CreateAsync(ProductImage productImage);
        Task<bool> DeleteAsync(int id);
        Task<bool> SetMainImageAsync(int productId, int imageId);
        Task<bool> UpdateDisplayOrderAsync(int imageId, int displayOrder);
        Task<bool> UpdateMultipleDisplayOrderAsync(Dictionary<int, int> imageOrders);
        Task<ProductImage?> GetMainImageAsync(int productId);
        Task<ProductImage?> UpdateAsync(int id, ProductImage productImage);
    }
}