using Utils.DTOs.Auth;

namespace Service.Auth
{
    public interface IAuthService
    {
        Task<TokenResponseDto> LoginAsync(LoginRequestDto request);
        Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    }
}
