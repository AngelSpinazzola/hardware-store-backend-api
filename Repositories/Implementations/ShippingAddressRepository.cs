using EcommerceAPI.Data;
using EcommerceAPI.Models;
using EcommerceAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace EcommerceAPI.Repositories.Implementations
{
    public class ShippingAddressRepository : IShippingAddressRepository
    {
        private readonly ApplicationDbContext _context;

        public ShippingAddressRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ShippingAddress>> GetByUserIdAsync(int userId)
        {
            try
            {
                return await _context.ShippingAddresses
                    .Where(sa => sa.UserId == userId && sa.IsActive)
                    .OrderByDescending(sa => sa.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving addresses for user: {UserId}", userId);
                return Enumerable.Empty<ShippingAddress>();
            }
        }

        public async Task<ShippingAddress?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.ShippingAddresses
                    .FirstOrDefaultAsync(sa => sa.Id == id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving address: {AddressId}", id);
                return null;
            }
        }

        public async Task<ShippingAddress> CreateAsync(ShippingAddress address)
        {
            try
            {
                _context.ShippingAddresses.Add(address);
                await _context.SaveChangesAsync();

                Log.Information("Address created: AddressId={AddressId}, UserId={UserId}",
                    address.Id, address.UserId);

                return address;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating address for user: {UserId}", address.UserId);
                throw;
            }
        }

        public async Task<ShippingAddress?> UpdateAsync(int id, ShippingAddress address)
        {
            try
            {
                var existingAddress = await _context.ShippingAddresses.FindAsync(id);
                if (existingAddress == null)
                {
                    Log.Warning("Address not found for update: {AddressId}", id);
                    return null;
                }

                // Actualizar propiedades
                existingAddress.AddressType = address.AddressType;
                existingAddress.Street = address.Street;
                existingAddress.Number = address.Number;
                existingAddress.Floor = address.Floor;
                existingAddress.Apartment = address.Apartment;
                existingAddress.Tower = address.Tower;
                existingAddress.BetweenStreets = address.BetweenStreets;
                existingAddress.PostalCode = address.PostalCode;
                existingAddress.Province = address.Province;
                existingAddress.City = address.City;
                existingAddress.Observations = address.Observations;
                existingAddress.IsActive = address.IsActive;
                existingAddress.UpdatedAt = address.UpdatedAt;

                await _context.SaveChangesAsync();

                Log.Information("Address updated: AddressId={AddressId}", id);

                return existingAddress;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating address: {AddressId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var address = await _context.ShippingAddresses.FindAsync(id);
                if (address == null)
                {
                    Log.Warning("Address not found for deletion: {AddressId}", id);
                    return false;
                }

                _context.ShippingAddresses.Remove(address);
                await _context.SaveChangesAsync();

                Log.Information("Address deleted: AddressId={AddressId}", id);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting address: {AddressId}", id);
                return false;
            }
        }
    }
}
