using EcommerceAPI.Models;

namespace EcommerceAPI.Repositories.Interfaces
{
    public interface IShippingAddressRepository
    {
        Task<IEnumerable<ShippingAddress>> GetByUserIdAsync(int userId);
        Task<ShippingAddress?> GetByIdAsync(int id);
        Task<ShippingAddress?> GetDefaultAddressAsync(int userId);
        Task<ShippingAddress> CreateAsync(ShippingAddress address);
        Task<ShippingAddress?> UpdateAsync(int id, ShippingAddress address);
        Task<bool> DeleteAsync(int id);
    }
}
