using EcommerceAPI.Services.Interfaces;
using Google.Apis.Auth;
using Serilog;

namespace EcommerceAPI.Services.Implementations
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;

        public GoogleAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { _configuration["Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                return payload;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error validating Google token");
                throw new UnauthorizedAccessException("Token de Google inválido");
            }
        }
    }
}