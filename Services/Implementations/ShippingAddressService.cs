using EcommerceAPI.DTOs.Customers;
using EcommerceAPI.Helpers;
using EcommerceAPI.Models;
using EcommerceAPI.Repositories.Interfaces;
using EcommerceAPI.Services.Interfaces;
using Serilog;

namespace EcommerceAPI.Services.Implementations
{
    public class ShippingAddressService : IShippingAddressService
    {
        private readonly IShippingAddressRepository _shippingAddressRepository;

        public ShippingAddressService(IShippingAddressRepository shippingAddressRepository)
        {
            _shippingAddressRepository = shippingAddressRepository;
        }

        public async Task<IEnumerable<ShippingAddressDto>> GetAddressesByUserIdAsync(int userId)
        {
            if (!SecurityHelper.IsValidId(userId))
            {
                Log.Warning("Invalid user ID for addresses: {UserId}", userId);
                return Enumerable.Empty<ShippingAddressDto>();
            }

            var addresses = await _shippingAddressRepository.GetByUserIdAsync(userId);
            Log.Information("Retrieved addresses for user: UserId={UserId}, Count={Count}", userId, addresses.Count());
            return addresses.Select(MapToShippingAddressDto);
        }

        public async Task<ShippingAddressDto?> GetAddressByIdAsync(int addressId, int userId)
        {
            if (!SecurityHelper.AreValidIds(addressId, userId))
            {
                Log.Warning("Invalid IDs for address: AddressId={AddressId}, UserId={UserId}", addressId, userId);
                return null;
            }

            var address = await _shippingAddressRepository.GetByIdAsync(addressId);

            // Verifica que la dirección pertenezca al usuario
            if (address == null || address.UserId != userId || !address.IsActive)
            {
                Log.Warning("Address not found or unauthorized: AddressId={AddressId}, UserId={UserId}", addressId, userId);
                return null;
            }

            return MapToShippingAddressDto(address);
        }

        public async Task<ShippingAddressDto> CreateAddressAsync(CreateShippingAddressDto createAddressDto, int userId)
        {
            if (!SecurityHelper.IsValidId(userId))
            {
                throw new ArgumentException("ID de usuario inválido");
            }

            // Valida y sanitiza datos
            var sanitizedAddress = SanitizeCreateAddressDto(createAddressDto);

            if (!IsValidProvince(sanitizedAddress.Province))
            {
                throw new ArgumentException("Provincia inválida");
            }
            if (!IsValidArgentinianPostalCode(sanitizedAddress.PostalCode))
            {
                throw new ArgumentException("Código postal inválido");
            }
            if (!IsValidArgentinianDni(sanitizedAddress.AuthorizedPersonDni))
            {
                throw new ArgumentException("Formato de DNI inválido");
            }

            var address = new ShippingAddress
            {
                UserId = userId,
                AddressType = sanitizedAddress.AddressType,
                Street = sanitizedAddress.Street,
                Number = sanitizedAddress.Number,
                Floor = sanitizedAddress.Floor,
                Apartment = sanitizedAddress.Apartment,
                Tower = sanitizedAddress.Tower,
                BetweenStreets = sanitizedAddress.BetweenStreets,
                PostalCode = sanitizedAddress.PostalCode,
                Province = sanitizedAddress.Province,
                City = sanitizedAddress.City,
                Observations = sanitizedAddress.Observations,
                AuthorizedPersonFirstName = sanitizedAddress.AuthorizedPersonFirstName,
                AuthorizedPersonLastName = sanitizedAddress.AuthorizedPersonLastName,
                AuthorizedPersonPhone = sanitizedAddress.AuthorizedPersonPhone,
                AuthorizedPersonDni = sanitizedAddress.AuthorizedPersonDni,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdAddress = await _shippingAddressRepository.CreateAsync(address);

            Log.Information("Address created: AddressId={AddressId}, UserId={UserId}, Type={Type}",
                createdAddress.Id, userId, createdAddress.AddressType);

            return MapToShippingAddressDto(createdAddress);
        }

        public async Task<bool> UpdateAddressAsync(int addressId, UpdateShippingAddressDto updateAddressDto, int userId)
        {
            if (!SecurityHelper.AreValidIds(addressId, userId))
            {
                Log.Warning("Invalid IDs for address update: AddressId={AddressId}, UserId={UserId}", addressId, userId);
                return false;
            }

            var address = await _shippingAddressRepository.GetByIdAsync(addressId);

            if (address == null || address.UserId != userId || !address.IsActive)
            {
                Log.Warning("Address not found or unauthorized for update: AddressId={AddressId}, UserId={UserId}", addressId, userId);
                return false;
            }

            // Valida y sanitiza datos
            var sanitizedAddress = SanitizeUpdateAddressDto(updateAddressDto);

            if (!IsValidProvince(sanitizedAddress.Province))
            {
                throw new ArgumentException("Provincia inválida");
            }
            if (!IsValidArgentinianPostalCode(sanitizedAddress.PostalCode))
            {
                throw new ArgumentException("Código postal inválido");
            }
            if (!IsValidArgentinianDni(sanitizedAddress.AuthorizedPersonDni))
            {
                throw new ArgumentException("Formato de DNI inválido");
            }

            address.AddressType = sanitizedAddress.AddressType;
            address.Street = sanitizedAddress.Street;
            address.Number = sanitizedAddress.Number;
            address.Floor = sanitizedAddress.Floor;
            address.Apartment = sanitizedAddress.Apartment;
            address.Tower = sanitizedAddress.Tower;
            address.BetweenStreets = sanitizedAddress.BetweenStreets;
            address.PostalCode = sanitizedAddress.PostalCode;
            address.Province = sanitizedAddress.Province;
            address.City = sanitizedAddress.City;
            address.Observations = sanitizedAddress.Observations;
            address.AuthorizedPersonFirstName = sanitizedAddress.AuthorizedPersonFirstName;
            address.AuthorizedPersonLastName = sanitizedAddress.AuthorizedPersonLastName;
            address.AuthorizedPersonPhone = sanitizedAddress.AuthorizedPersonPhone;
            address.AuthorizedPersonDni = sanitizedAddress.AuthorizedPersonDni;
            address.UpdatedAt = DateTime.UtcNow;

            var result = await _shippingAddressRepository.UpdateAsync(addressId, address) != null;

            if (result)
            {
                Log.Information("Address updated: AddressId={AddressId}, UserId={UserId}", addressId, userId);
            }

            return result;
        }

        public async Task<bool> DeleteAddressAsync(int addressId, int userId)
        {
            if (!SecurityHelper.AreValidIds(addressId, userId))
            {
                Log.Warning("Invalid IDs for address deletion: AddressId={AddressId}, UserId={UserId}", addressId, userId);
                return false;
            }

            var address = await _shippingAddressRepository.GetByIdAsync(addressId);
            if (address == null || address.UserId != userId)
            {
                Log.Warning("Address not found or unauthorized for deletion: AddressId={AddressId}, UserId={UserId}", addressId, userId);
                return false;
            }

            // Soft delete
            address.IsActive = false;
            address.UpdatedAt = DateTime.UtcNow;

            var result = await _shippingAddressRepository.UpdateAsync(addressId, address) != null;

            if (result)
            {
                Log.Information("Address deleted: AddressId={AddressId}, UserId={UserId}", addressId, userId);
            }

            return result;
        }

        private CreateShippingAddressDto SanitizeCreateAddressDto(CreateShippingAddressDto dto)
        {
            return new CreateShippingAddressDto
            {
                AddressType = SecurityHelper.SanitizeCategory(dto.AddressType),
                Street = SecurityHelper.SanitizeInput(dto.Street),
                Number = SecurityHelper.SanitizeInput(dto.Number),
                Floor = SecurityHelper.SanitizeInput(dto.Floor),
                Apartment = SecurityHelper.SanitizeInput(dto.Apartment),
                Tower = SecurityHelper.SanitizeInput(dto.Tower),
                BetweenStreets = SecurityHelper.SanitizeInput(dto.BetweenStreets),
                PostalCode = SecurityHelper.SanitizeInput(dto.PostalCode),
                Province = SecurityHelper.SanitizeInput(dto.Province),
                City = SecurityHelper.SanitizeInput(dto.City),
                Observations = SecurityHelper.SanitizeInput(dto.Observations),
                AuthorizedPersonFirstName = SecurityHelper.SanitizeName(dto.AuthorizedPersonFirstName),
                AuthorizedPersonLastName = SecurityHelper.SanitizeName(dto.AuthorizedPersonLastName),
                AuthorizedPersonPhone = SecurityHelper.SanitizePhone(dto.AuthorizedPersonPhone),
                AuthorizedPersonDni = SecurityHelper.SanitizeInput(dto.AuthorizedPersonDni)
            };
        }

        private UpdateShippingAddressDto SanitizeUpdateAddressDto(UpdateShippingAddressDto dto)
        {
            return new UpdateShippingAddressDto
            {
                AddressType = SecurityHelper.SanitizeCategory(dto.AddressType),
                Street = SecurityHelper.SanitizeInput(dto.Street),
                Number = SecurityHelper.SanitizeInput(dto.Number),
                Floor = SecurityHelper.SanitizeInput(dto.Floor),
                Apartment = SecurityHelper.SanitizeInput(dto.Apartment),
                Tower = SecurityHelper.SanitizeInput(dto.Tower),
                BetweenStreets = SecurityHelper.SanitizeInput(dto.BetweenStreets),
                PostalCode = SecurityHelper.SanitizeInput(dto.PostalCode),
                Province = SecurityHelper.SanitizeInput(dto.Province),
                City = SecurityHelper.SanitizeInput(dto.City),
                Observations = SecurityHelper.SanitizeInput(dto.Observations),
                AuthorizedPersonFirstName = SecurityHelper.SanitizeName(dto.AuthorizedPersonFirstName),
                AuthorizedPersonLastName = SecurityHelper.SanitizeName(dto.AuthorizedPersonLastName),
                AuthorizedPersonPhone = SecurityHelper.SanitizePhone(dto.AuthorizedPersonPhone),
                AuthorizedPersonDni = SecurityHelper.SanitizeInput(dto.AuthorizedPersonDni)
            };
        }

        private ShippingAddressDto MapToShippingAddressDto(ShippingAddress address)
        {
            return new ShippingAddressDto
            {
                Id = address.Id,
                UserId = address.UserId,
                AddressType = address.AddressType,
                Street = address.Street,
                Number = address.Number,
                Floor = address.Floor,
                Apartment = address.Apartment,
                Tower = address.Tower,
                BetweenStreets = address.BetweenStreets,
                PostalCode = address.PostalCode,
                Province = address.Province,
                City = address.City,
                Observations = address.Observations,
                AuthorizedPersonFirstName = address.AuthorizedPersonFirstName,
                AuthorizedPersonLastName = address.AuthorizedPersonLastName,
                AuthorizedPersonPhone = address.AuthorizedPersonPhone,
                AuthorizedPersonDni = address.AuthorizedPersonDni,
                IsActive = address.IsActive,
                CreatedAt = address.CreatedAt,
                UpdatedAt = address.UpdatedAt
            };
        }

        private bool IsValidProvince(string province)
        {
            var validProvinces = new[]
            {
                "Buenos Aires", "CABA", "Catamarca", "Chaco", "Chubut", "Córdoba",
                "Corrientes", "Entre Ríos", "Formosa", "Jujuy", "La Pampa", "La Rioja",
                "Mendoza", "Misiones", "Neuquén", "Río Negro", "Salta", "San Juan",
                "San Luis", "Santa Cruz", "Santa Fe", "Santiago del Estero",
                "Tierra del Fuego", "Tucumán"
            };

            return validProvinces.Contains(province);
        }

        private bool IsValidArgentinianPostalCode(string postalCode)
        {
            return !string.IsNullOrEmpty(postalCode) &&
                   postalCode.Length >= 4 &&
                   postalCode.All(c => char.IsDigit(c) || c == ' ' || c == '-');
        }

        private bool IsValidArgentinianDni(string dni)
        {
            if (string.IsNullOrEmpty(dni)) return false;

            // Valida que tenga exactamente entre 7 y 8 dígitos sin espacios
            return dni.Length >= 7 &&
                   dni.Length <= 8 &&
                   dni.All(char.IsDigit);
        }

    }
}