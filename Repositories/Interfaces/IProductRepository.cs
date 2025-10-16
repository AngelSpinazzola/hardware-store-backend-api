using EcommerceAPI.Models;

namespace EcommerceAPI.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetAllForAdminAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<Product> CreateAsync(Product product);
        Task<Product?> UpdateAsync(int id, Product product);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Product>> GetByCategoryAsync(string category);
        Task<IEnumerable<Product>> SearchAsync(string searchTerm);
        Task<IEnumerable<string>> GetCategoriesAsync();
        Task<IEnumerable<Product>> GetByBrandAsync(string brand);
        Task<IEnumerable<string>> GetBrandsAsync();
        Task<IEnumerable<Product>> FilterAsync(
            string? category = null, 
            string? brand = null, 
            decimal? minPrice = null, 
            decimal? maxPrice = null, 
            bool? inStock = null);
        Task<Dictionary<string, Dictionary<string, List<string>>>> GetMenuStructureAsync();
        Task<object> GetProductStatsAsync();
    }
}