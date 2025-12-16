using HardwareStore.Domain.Entities;

namespace HardwareStore.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> CreateAsync(User user);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByGoogleIdAsync(string googleId);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> UpdateAsync(int id, User user);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string email);
        Task UpdateLastLoginAsync(int userId);
    }
}
