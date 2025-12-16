using Google.Apis.Auth;

namespace HardwareStore.Application.Common.Interfaces
{
    public interface IGoogleAuthService
    {
        Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken);
        Task<GoogleJsonWebSignature.Payload> ValidateGoogleAccessTokenAsync(string accessToken);
    }
}