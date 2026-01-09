using HardwareStore.Domain.Entities;

namespace HardwareStore.Domain.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<Category?> GetByNameAsync(string name);
        Task<Category> CreateAsync(Category category);
        Task<Category?> UpdateAsync(int id, Category category);
        Task<bool> DeleteAsync(int id);
    }
}
