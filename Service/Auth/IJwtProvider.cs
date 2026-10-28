using Biblioteca.Entities;
using Utils.DTOs.Auth;

namespace Service.Auth
{
    public interface IJwtProvider
    {
        TokenResponseDto GenerateTokens(Profesional profesional);
    }
}
