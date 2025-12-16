using HardwareStore.Domain.Entities;

namespace HardwareStore.Domain.Interfaces
{
    public interface IShippingAddressRepository
    {
        Task<IEnumerable<ShippingAddress>> GetByUserIdAsync(int userId);
        Task<ShippingAddress?> GetByIdAsync(int id);
        Task<ShippingAddress> CreateAsync(ShippingAddress address);
        Task<ShippingAddress?> UpdateAsync(int id, ShippingAddress address);
        Task<bool> DeleteAsync(int id);
    }
}
