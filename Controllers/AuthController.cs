using EcommerceAPI.Data;
using EcommerceAPI.Helpers;
using EcommerceAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Serilog;
using EcommerceAPI.DTOs.Auth;
using EcommerceAPI.DTOs.Common;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtHelper _jwtHelper;

        public AuthController(ApplicationDbContext context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }

        [HttpPost("register")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Log.Warning("Invalid registration attempt from IP: {IP}",
                        HttpContext.Connection.RemoteIpAddress);
                    return BadRequest(ModelState);
                }

                // SecurityHelper para validaciones
                var validationResult = SecurityHelper.ValidateRegistrationData(
                    registerDto.Email,
                    registerDto.Password,
                    registerDto.FirstName,
                    registerDto.LastName);

                if (!validationResult.IsValid)
                {
                    Log.Warning("Registration validation failed: {Error} from IP: {IP}",
                        validationResult.ErrorMessage, HttpContext.Connection.RemoteIpAddress);
                    return BadRequest(new { message = validationResult.ErrorMessage });
                }

                // Normaliza email (simple)
                var normalizedEmail = registerDto.Email.Trim().ToLowerInvariant();

                // Verifica si el email ya existe
                if (await _context.Users.AnyAsync(u => u.Email == normalizedEmail))
                {
                    Log.Warning("Registration attempt with existing email: {Email} from IP: {IP}",
                        normalizedEmail, HttpContext.Connection.RemoteIpAddress);
                    return BadRequest(new { message = "El email ya está registrado" });
                }

                var user = new User
                {
                    Email = normalizedEmail,
                    PasswordHash = PasswordHelper.HashPassword(registerDto.Password),
                    FirstName = registerDto.FirstName.Trim(),
                    LastName = registerDto.LastName.Trim(),
                    Role = "Customer",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Genera token JWT
                var token = _jwtHelper.GenerateToken(user.Id, user.Email, user.Role);

                // Configuración de cookie httpOnly
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(7), 
                    Path = "/"
                };

                Response.Cookies.Append("token", token, cookieOptions);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role
                };

                Log.Information("User registered successfully: UserId={UserId}, Email={Email}",
                    user.Id, user.Email);

                return Ok(new
                {
                    user = userDto,
                    message = "Registro exitoso"
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Registration failed for email: {Email}", registerDto?.Email);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Log.Warning("Invalid login attempt from IP: {IP}",
                        HttpContext.Connection.RemoteIpAddress);
                    return BadRequest(ModelState);
                }

                // Valida entrada básica
                if (!SecurityHelper.IsNotEmptyOrWhitespace(loginDto.Email) ||
                    !SecurityHelper.IsNotEmptyOrWhitespace(loginDto.Password))
                {
                    Log.Warning("Login attempt with empty credentials from IP: {IP}",
                        HttpContext.Connection.RemoteIpAddress);
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                // Normaliza email
                var normalizedEmail = loginDto.Email.Trim().ToLowerInvariant();

                Log.Information("Login attempt for email: {Email} from IP: {IP}",
                    normalizedEmail, HttpContext.Connection.RemoteIpAddress);

                // Busca usuario por email
                var user = await _context.Users
                    .Where(u => u.Email == normalizedEmail && u.IsActive)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    Log.Warning("Login failed - User not found: {Email} from IP: {IP}",
                        normalizedEmail, HttpContext.Connection.RemoteIpAddress);

                    // Protección contra timing attacks: simular verificación de password
                    PasswordHelper.VerifyPassword("dummy", "$2a$11$dummy.hash.to.prevent.timing.attacks");

                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                // Verifica contraseña
                if (!PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    Log.Warning("Login failed - Invalid password for user: {UserId} from IP: {IP}",
                        user.Id, HttpContext.Connection.RemoteIpAddress);
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                // Actualiza último login
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Genera token JWT
                var token = _jwtHelper.GenerateToken(user.Id, user.Email, user.Role);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(7), 
                    Path = "/"
                };

                Response.Cookies.Append("token", token, cookieOptions);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role
                };

                Log.Information("Login successful for user: {UserId} from IP: {IP}",
                    user.Id, HttpContext.Connection.RemoteIpAddress);

                return Ok(new
                {
                    user = userDto,
                    message = "Login exitoso"
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Login error for email: {Email} from IP: {IP}",
                    loginDto?.Email, HttpContext.Connection.RemoteIpAddress);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            try
            {
                // Elimina cookie del token
                Response.Cookies.Delete("token");

                Log.Information("User logged out successfully");

                return Ok(new { message = "Logout exitoso" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Logout error");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // Devuelve información del usuario actual
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                // OrderAuthorizationHelper para extraer ID
                var userInfo = OrderAuthorizationHelper.GetUserIdFromClaims(User);
                if (!userInfo.IsValid)
                {
                    Log.Warning("Invalid user ID claim: {Claim}",
                        User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    return BadRequest(new { message = "Token inválido" });
                }

                var user = await _context.Users
                    .Where(u => u.Id == userInfo.UserId && u.IsActive)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Role = u.Role
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    Log.Warning("User not found or inactive: {UserId}", userInfo.UserId);
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving current user: {UserId}",
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateUserDto updateUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Log.Warning("Invalid profile update attempt by user: {UserId}",
                        User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    return BadRequest(ModelState);
                }

                // Usa helper para obtener ID
                var userInfo = OrderAuthorizationHelper.GetUserIdFromClaims(User);
                if (!userInfo.IsValid)
                {
                    Log.Warning("Invalid user ID in profile update: {Claim}",
                        User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    return BadRequest(new { message = "Token inválido" });
                }

                var user = await _context.Users
                    .Where(u => u.Id == userInfo.UserId && u.IsActive)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    Log.Warning("User not found for profile update: {UserId}", userInfo.UserId);
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // Valida datos básicos
                user.FirstName = SecurityHelper.SanitizeName(updateUserDto.FirstName);
                user.LastName = SecurityHelper.SanitizeName(updateUserDto.LastName);


                await _context.SaveChangesAsync();

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role
                };

                Log.Information("Profile updated successfully for user: {UserId}", userInfo.UserId);

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Profile update failed for user: {UserId}",
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("change-password")]
        [Authorize]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Usa helper para obtener ID
                var userInfo = OrderAuthorizationHelper.GetUserIdFromClaims(User);
                if (!userInfo.IsValid)
                {
                    return BadRequest(new { message = "Token inválido" });
                }

                var user = await _context.Users
                    .Where(u => u.Id == userInfo.UserId && u.IsActive)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // Verifica contraseña actual
                if (!PasswordHelper.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
                {
                    Log.Warning("Invalid current password in change password attempt: UserId={UserId}",
                        userInfo.UserId);
                    return BadRequest(new { message = "Contraseña actual incorrecta" });
                }

                // Valida nueva contraseña usando SecurityHelper
                var passwordValidation = SecurityHelper.ValidatePassword(changePasswordDto.NewPassword);
                if (!passwordValidation.IsValid)
                {
                    return BadRequest(new { message = passwordValidation.ErrorMessage });
                }

                // Actualiza contraseña
                user.PasswordHash = PasswordHelper.HashPassword(changePasswordDto.NewPassword);
                await _context.SaveChangesAsync();

                Log.Information("Password changed successfully for user: {UserId}", userInfo.UserId);

                return Ok(new { message = "Contraseña actualizada correctamente" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Password change failed for user: {UserId}",
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}