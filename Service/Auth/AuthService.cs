using Repository.Profesionales;
using Utils.DTOs.Auth;
using Utils.Exceptions;
using Microsoft.Extensions.Configuration;
using Biblioteca.Entities;

namespace Service.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IProfesionalRepository _profesionalRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IConfiguration _configuration;

        public AuthService(IProfesionalRepository profesionalRepository, IJwtProvider jwtProvider, IConfiguration configuration)
        {
            _profesionalRepository = profesionalRepository;
            _jwtProvider = jwtProvider;
            _configuration = configuration;
        }

        public async Task<TokenResponseDto> LoginAsync(LoginRequestDto request)
        {
            var profesional = await _profesionalRepository.GetByEmailAsync(request.Email);
            if (profesional == null)
            {
                throw new UnauthorizedError("Credenciales inválidas.");
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, profesional.Password);
            if (!isPasswordValid)
            {
                throw new UnauthorizedError("Credenciales inválidas.");
            }

            var tokens = _jwtProvider.GenerateTokens(profesional);

            await UpdateRefreshTokenAsync(profesional, tokens);

            return tokens;
        }

        public async Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var profesional = await _profesionalRepository.GetByRefreshTokenAsync(request.RefreshToken);
            
            if (profesional == null || profesional.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnauthorizedError("Refresh token inválido o expirado.");
            }

            var tokens = _jwtProvider.GenerateTokens(profesional);

            await UpdateRefreshTokenAsync(profesional, tokens);

            return tokens;
        }
        private async Task UpdateRefreshTokenAsync(Profesional profesional, TokenResponseDto tokens)
        {
            profesional.RefreshToken = tokens.RefreshToken;
            var days = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");
            profesional.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(days);

            await _profesionalRepository.UpdateAsync(profesional);
            await _profesionalRepository.GuardarCambios();
        }
    }
}
