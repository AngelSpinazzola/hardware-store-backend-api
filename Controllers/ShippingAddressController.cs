using EcommerceAPI.DTOs.Customers;
using EcommerceAPI.Helpers;
using EcommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Security.Claims;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Solo usuarios autenticados
    public class ShippingAddressController : ControllerBase
    {
        private readonly IShippingAddressService _shippingAddressService;

        public ShippingAddressController(IShippingAddressService shippingAddressService)
        {
            _shippingAddressService = shippingAddressService;
        }

        // Devuelve todas las direcciones del usuario
        [HttpGet("my-addresses")]
        public async Task<IActionResult> GetMyAddresses()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new { message = "Usuario no válido" });
                }

                var addresses = await _shippingAddressService.GetAddressesByUserIdAsync(userId);
                return Ok(addresses);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving user addresses for: {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // Devuelve dirección por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAddress(int id)
        {
            try
            {
                if (!SecurityHelper.IsValidId(id))
                {
                    return BadRequest(new { message = "ID de dirección inválido" });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new { message = "Usuario no válido" });
                }

                var address = await _shippingAddressService.GetAddressByIdAsync(id, userId);
                if (address == null)
                {
                    return NotFound(new { message = "Dirección no encontrada" });
                }

                return Ok(address);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving address: {AddressId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> CreateAddress([FromBody] CreateShippingAddressDto createAddressDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new { message = "Usuario no válido" });
                }

                var address = await _shippingAddressService.CreateAddressAsync(createAddressDto, userId);
                return CreatedAtAction(nameof(GetAddress), new { id = address.Id }, address);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating address for user: {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> UpdateAddress(int id, [FromBody] UpdateShippingAddressDto updateAddressDto)
        {
            try
            {
                if (!SecurityHelper.IsValidId(id))
                {
                    return BadRequest(new { message = "ID de dirección inválido" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new { message = "Usuario no válido" });
                }

                var result = await _shippingAddressService.UpdateAddressAsync(id, updateAddressDto, userId);
                if (!result)
                {
                    return NotFound(new { message = "Dirección no encontrada" });
                }

                return Ok(new { message = "Dirección actualizada correctamente" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating address: {AddressId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            try
            {
                if (!SecurityHelper.IsValidId(id))
                {
                    return BadRequest(new { message = "ID de dirección inválido" });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new { message = "Usuario no válido" });
                }

                var result = await _shippingAddressService.DeleteAddressAsync(id, userId);
                if (!result)
                {
                    return NotFound(new { message = "Dirección no encontrada" });
                }

                return Ok(new { message = "Dirección eliminada correctamente" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting address: {AddressId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // Establece dirección como predeterminada
        [HttpPut("{id}/set-default")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> SetDefaultAddress(int id)
        {
            try
            {
                if (!SecurityHelper.IsValidId(id))
                {
                    return BadRequest(new { message = "ID de dirección inválido" });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new { message = "Usuario no válido" });
                }

                var result = await _shippingAddressService.SetDefaultAddressAsync(id, userId);
                if (!result)
                {
                    return NotFound(new { message = "Dirección no encontrada" });
                }

                return Ok(new { message = "Dirección establecida como predeterminada" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error setting default address: {AddressId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    }
}