using EcommerceAPI.Models;

namespace EcommerceAPI.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> CreateAsync(Order order);
        Task<Order?> GetByIdAsync(int id);
        Task<IEnumerable<Order>> GetAllAsync();
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
        Task<Order?> UpdateAsync(int id, Order order);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<IEnumerable<Order>> GetByStatusAsync(string status);
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10);
    }
}
