using Google.Apis.Auth;

namespace EcommerceAPI.Services.Interfaces
{
    public interface IGoogleAuthService
    {
        Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken);
        Task<GoogleJsonWebSignature.Payload> ValidateGoogleAccessTokenAsync(string accessToken);
    }
}