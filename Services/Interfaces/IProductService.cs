using EcommerceAPI.DTOs.Products;

namespace EcommerceAPI.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductListDto>> GetAllProductsAsync();
        Task<IEnumerable<ProductListDto>> GetAllProductsForAdminAsync();
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
        Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
        Task<bool> DeleteProductAsync(int id);
        Task<IEnumerable<ProductListDto>> GetProductsByCategoryAsync(string category);
        Task<IEnumerable<ProductListDto>> SearchProductsAsync(string searchTerm);
        Task<IEnumerable<string>> GetCategoriesAsync();
        Task<IEnumerable<ProductListDto>> GetProductsByBrandAsync(string brand);
        Task<IEnumerable<string>> GetBrandsAsync();
        Task<IEnumerable<ProductListDto>> FilterProductsAsync(
            string? category = null, 
            string? brand = null, 
            decimal? minPrice = null, 
            decimal? maxPrice = null, 
            bool? inStock = null);
        Task<object> GetProductMenuStructureAsync();
        Task<object> GetProductStatsAsync();
    }
}