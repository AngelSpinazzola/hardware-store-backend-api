using EcommerceAPI.DTOs.Customers;

namespace EcommerceAPI.Services.Interfaces
{
    public interface IShippingAddressService
    {
        Task<IEnumerable<ShippingAddressDto>> GetAddressesByUserIdAsync(int userId);
        Task<ShippingAddressDto?> GetAddressByIdAsync(int addressId, int userId);
        Task<ShippingAddressDto> CreateAddressAsync(CreateShippingAddressDto createAddressDto, int userId);
        Task<bool> UpdateAddressAsync(int addressId, UpdateShippingAddressDto updateAddressDto, int userId);
        Task<bool> DeleteAddressAsync(int addressId, int userId);
        Task<bool> SetDefaultAddressAsync(int addressId, int userId);
        Task<ShippingAddressDto?> GetDefaultAddressAsync(int userId);
    }
}
