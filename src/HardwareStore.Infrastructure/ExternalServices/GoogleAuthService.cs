using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using HardwareStore.Application.Customers;
using Microsoft.AspNetCore.Http;
using HardwareStore.Application.Auth;
using Google.Apis.Auth;
using Serilog;
using System.Text.Json;

namespace HardwareStore.Infrastructure.ExternalServices
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public GoogleAuthService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
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

        public async Task<GoogleJsonWebSignature.Payload> ValidateGoogleAccessTokenAsync(string accessToken)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://www.googleapis.com/oauth2/v3/userinfo?access_token={accessToken}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new UnauthorizedAccessException("Invalid access token");
                }

                var json = await response.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<JsonElement>(json);

                return new GoogleJsonWebSignature.Payload
                {
                    Subject = userInfo.GetProperty("sub").GetString(),
                    Email = userInfo.GetProperty("email").GetString(),
                    GivenName = userInfo.TryGetProperty("given_name", out var gn) ? gn.GetString() : null,
                    FamilyName = userInfo.TryGetProperty("family_name", out var fn) ? fn.GetString() : null,
                    Picture = userInfo.TryGetProperty("picture", out var pic) ? pic.GetString() : null
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error validating Google access token");
                throw new UnauthorizedAccessException("Access token de Google inválido");
            }
        }
    }
}