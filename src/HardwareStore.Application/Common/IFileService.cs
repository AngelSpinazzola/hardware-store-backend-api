using Microsoft.AspNetCore.Http;

namespace HardwareStore.Application.Common
{
    public interface IFileService
    {
        Task<string> SaveImageAsync(IFormFile imageFile, string folder = "products");
        Task<bool> DeleteImageAsync(string imagePath);
        bool IsValidImageFile(IFormFile file, bool isProductImage = false);
        bool IsValidReceiptFile(IFormFile file);
        Task<List<string>> SaveMultipleImagesAsync(IFormFile[] imageFiles, string folder = "products");
    }
}
