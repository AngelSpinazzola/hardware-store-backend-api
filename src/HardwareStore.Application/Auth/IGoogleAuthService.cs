using Google.Apis.Auth;

namespace HardwareStore.Application.Auth
{
    public interface IGoogleAuthService
    {
        Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken);
        Task<GoogleJsonWebSignature.Payload> ValidateGoogleAccessTokenAsync(string accessToken);
    }
}
