using Biblioteca.Entities;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Repository.Profesionales;
using Service.Auth;
using Utils.DTOs.Auth;
using Utils.Enums;
using Utils.Exceptions;

namespace Service.Tests.Auth;

public class AuthServiceTests
{
    private const string PasswordPlano = "Secreta123!";
    private static readonly string PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordPlano);

    private readonly IProfesionalRepository _profesionalRepository = Substitute.For<IProfesionalRepository>();
    private readonly IJwtProvider _jwtProvider = Substitute.For<IJwtProvider>();

    private AuthService CrearServicio(string diasExpiracion = "7")
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:RefreshTokenExpirationDays"] = diasExpiracion
            })
            .Build();

        return new AuthService(_profesionalRepository, _jwtProvider, configuration);
    }

    private static Profesional ProfesionalConPassword(string? email = "ana@meditool.test")
    {
        var profesional = new Profesional
        {
            Id = 7,
            Nombre = "Ana",
            Apellido = "Gomez",
            Email = email,
            Password = PasswordHash,
            Rol = Rol.Profesional
        };
        return profesional;
    }

    private void JwtDevuelve(string accessToken, string refreshToken) =>
        _jwtProvider.GenerateTokens(Arg.Any<Profesional>())
            .Returns(new TokenResponseDto { AccessToken = accessToken, RefreshToken = refreshToken });

    // ----- Login -----

    [Fact]
    public async Task LoginAsync_EmailInexistente_LanzaUnauthorizedError()
    {
        // Arrange
        _profesionalRepository.GetByEmailAsync("nadie@meditool.test").Returns((Profesional?)null);

        // Act
        var accion = () => CrearServicio().LoginAsync(new LoginRequestDto
        {
            Email = "nadie@meditool.test",
            Password = PasswordPlano
        });

        // Assert
        await accion.Should().ThrowAsync<UnauthorizedError>();
    }

    [Fact]
    public async Task LoginAsync_PasswordIncorrecta_LanzaUnauthorizedError()
    {
        // Arrange
        _profesionalRepository.GetByEmailAsync("ana@meditool.test").Returns(ProfesionalConPassword());

        // Act
        var accion = () => CrearServicio().LoginAsync(new LoginRequestDto
        {
            Email = "ana@meditool.test",
            Password = "password-incorrecta"
        });

        // Assert
        await accion.Should().ThrowAsync<UnauthorizedError>();
    }

    [Fact]
    public async Task LoginAsync_CredencialesValidas_EmiteTokensYPersisteElRefreshToken()
    {
        // Arrange
        var profesional = ProfesionalConPassword();
        _profesionalRepository.GetByEmailAsync("ana@meditool.test").Returns(profesional);
        JwtDevuelve("access-token", "refresh-token-nuevo");
        var antes = DateTime.UtcNow;

        // Act
        var resultado = await CrearServicio().LoginAsync(new LoginRequestDto
        {
            Email = "ana@meditool.test",
            Password = PasswordPlano
        });

        // Assert
        resultado.AccessToken.Should().Be("access-token");
        resultado.RefreshToken.Should().Be("refresh-token-nuevo");
        profesional.RefreshToken.Should().Be("refresh-token-nuevo");
        profesional.RefreshTokenExpiryTime.Should().NotBeNull();
        profesional.RefreshTokenExpiryTime!.Value.Should().BeCloseTo(antes.AddDays(7), TimeSpan.FromMinutes(1));
        await _profesionalRepository.Received(1).GuardarCambios();
    }

    [Fact]
    public async Task LoginAsync_SinConfiguracionDeExpiracion_UsaElValorPorDefectoDeSieteDias()
    {
        // Arrange
        var profesional = ProfesionalConPassword();
        _profesionalRepository.GetByEmailAsync("ana@meditool.test").Returns(profesional);
        _jwtProvider.GenerateTokens(Arg.Any<Profesional>())
            .Returns(new TokenResponseDto { AccessToken = "a", RefreshToken = "r" });
        var configuracionSinClave = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        var servicio = new AuthService(_profesionalRepository, _jwtProvider, configuracionSinClave);
        var antes = DateTime.UtcNow;

        // Act
        await servicio.LoginAsync(new LoginRequestDto { Email = "ana@meditool.test", Password = PasswordPlano });

        // Assert
        profesional.RefreshTokenExpiryTime!.Value.Should().BeCloseTo(antes.AddDays(7), TimeSpan.FromMinutes(1));
    }

    // ----- Refresh token -----

    [Fact]
    public async Task RefreshTokenAsync_TokenInexistente_LanzaUnauthorizedError()
    {
        // Arrange
        _profesionalRepository.GetByRefreshTokenAsync("token-fantasma").Returns((Profesional?)null);

        // Act
        var accion = () => CrearServicio().RefreshTokenAsync(new RefreshTokenRequestDto { RefreshToken = "token-fantasma" });

        // Assert
        await accion.Should().ThrowAsync<UnauthorizedError>();
    }

    [Fact]
    public async Task RefreshTokenAsync_TokenRevocadoOInvalido_LanzaUnauthorizedError()
    {
        // Arrange: un token que fue revocado ya no se encuentra en la base
        _profesionalRepository.GetByRefreshTokenAsync("token-revocado").Returns((Profesional?)null);

        // Act
        var accion = () => CrearServicio().RefreshTokenAsync(new RefreshTokenRequestDto { RefreshToken = "token-revocado" });

        // Assert
        await accion.Should().ThrowAsync<UnauthorizedError>();
    }

    [Fact]
    public async Task RefreshTokenAsync_TokenExpirado_LanzaUnauthorizedError()
    {
        // Arrange
        var profesional = ProfesionalConPassword();
        profesional.RefreshToken = "viejo";
        profesional.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(-1);
        _profesionalRepository.GetByRefreshTokenAsync("viejo").Returns(profesional);

        // Act
        var accion = () => CrearServicio().RefreshTokenAsync(new RefreshTokenRequestDto { RefreshToken = "viejo" });

        // Assert
        await accion.Should().ThrowAsync<UnauthorizedError>();
    }

    [Fact]
    public async Task RefreshTokenAsync_TokenVigente_RotaElRefreshTokenYEmiteNuevosTokens()
    {
        // Arrange
        var profesional = ProfesionalConPassword();
        profesional.RefreshToken = "refresh-viejo";
        profesional.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(3);
        _profesionalRepository.GetByRefreshTokenAsync("refresh-viejo").Returns(profesional);
        JwtDevuelve("access-nuevo", "refresh-nuevo");
        var antes = DateTime.UtcNow;

        // Act
        var resultado = await CrearServicio().RefreshTokenAsync(new RefreshTokenRequestDto { RefreshToken = "refresh-viejo" });

        // Assert
        resultado.RefreshToken.Should().Be("refresh-nuevo");
        profesional.RefreshToken.Should().Be("refresh-nuevo");
        profesional.RefreshToken.Should().NotBe("refresh-viejo");
        profesional.RefreshTokenExpiryTime!.Value.Should().BeCloseTo(antes.AddDays(7), TimeSpan.FromMinutes(1));
        await _profesionalRepository.Received(1).GuardarCambios();
    }

    [Fact]
    public async Task RefreshTokenAsync_ConfiguracionDeExpiracionPersonalizada_AplicaLosDiasIndicados()
    {
        // Arrange
        var profesional = ProfesionalConPassword();
        profesional.RefreshToken = "refresh-viejo";
        profesional.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(3);
        _profesionalRepository.GetByRefreshTokenAsync("refresh-viejo").Returns(profesional);
        JwtDevuelve("a", "r");
        var antes = DateTime.UtcNow;

        // Act
        await CrearServicio(diasExpiracion: "30")
            .RefreshTokenAsync(new RefreshTokenRequestDto { RefreshToken = "refresh-viejo" });

        // Assert
        profesional.RefreshTokenExpiryTime!.Value.Should().BeCloseTo(antes.AddDays(30), TimeSpan.FromMinutes(1));
    }
}
