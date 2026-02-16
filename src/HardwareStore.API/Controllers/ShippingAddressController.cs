using HardwareStore.Application.Customers;
using HardwareStore.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Security.Claims;

namespace HardwareStore.API.Controllers
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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { message = "Usuario no válido" });
            }

            var addresses = await _shippingAddressService.GetAddressesByUserIdAsync(userId);
            return Ok(addresses);
        }

        // Devuelve dirección por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAddress(int id)
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

        [HttpPost]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> CreateAddress([FromBody] CreateShippingAddressDto createAddressDto)
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

        [HttpPut("{id}")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> UpdateAddress(int id, [FromBody] UpdateShippingAddressDto updateAddressDto)
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

        [HttpDelete("{id}")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> DeleteAddress(int id)
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
    }
}
