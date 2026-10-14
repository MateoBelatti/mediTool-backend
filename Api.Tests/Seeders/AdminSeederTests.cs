using Biblioteca.Entities;
using Biblioteca.Repository;
using mediTool.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Utils.Enums;

namespace Api.Tests.Seeders;

public class AdminSeederTests
{
    private static ApplicationDbContext CrearDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static IConfiguration CrearConfiguracion(string? email, string? password)
    {
        var dict = new Dictionary<string, string?>();
        if (email != null) dict["Seed:AdminEmail"] = email;
        if (password != null) dict["Seed:AdminPassword"] = password;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("   ", "   ")]
    [InlineData("admin@meditool.test", null)]
    [InlineData("admin@meditool.test", "")]
    [InlineData(null, "Admin123!")]
    [InlineData("", "Admin123!")]
    public void SeedAdmin_VariablesDeConfiguracionFaltantesOIncompletas_NoSiembraAdmin(string? email, string? password)
    {
        // Arrange
        using var context = CrearDbContext();
        var config = CrearConfiguracion(email, password);

        // Act
        AdminSeeder.SeedAdmin(context, config);

        // Assert
        context.Profesionales.Should().BeEmpty();
    }

    [Fact]
    public void SeedAdmin_ConCredencialesValidasYBaseVacia_CreaAdminConRolAdminYPasswordHasheado()
    {
        // Arrange
        using var context = CrearDbContext();
        const string email = "admin@meditool.test";
        const string password = "PasswordAdminSeguro123!";
        var config = CrearConfiguracion(email, password);

        // Act
        AdminSeeder.SeedAdmin(context, config);

        // Assert
        var admin = context.Profesionales.Should().ContainSingle().Subject;
        admin.Rol.Should().Be(Rol.Admin);
        admin.Email.Should().Be(email);
        admin.Nombre.Should().Be("Admin");
        admin.Apellido.Should().Be("Admin");
        BCrypt.Net.BCrypt.Verify(password, admin.Password).Should().BeTrue();
    }

    [Fact]
    public void SeedAdmin_YaExisteUnAdmin_EsIdempotenteYNoDuplicaElAdmin()
    {
        // Arrange
        using var context = CrearDbContext();
        var adminExistente = new Profesional
        {
            Id = 1,
            Nombre = "Super",
            Apellido = "Admin",
            Email = "existente@meditool.test",
            Password = "hash-existente",
            Rol = Rol.Admin
        };
        context.Profesionales.Add(adminExistente);
        context.SaveChanges();

        var config = CrearConfiguracion("nuevo_admin@meditool.test", "OtraPassword123!");

        // Act
        AdminSeeder.SeedAdmin(context, config);

        // Assert
        context.Profesionales.Should().ContainSingle();
        var adminEnDb = context.Profesionales.First();
        adminEnDb.Email.Should().Be("existente@meditool.test");
        adminEnDb.Nombre.Should().Be("Super");
    }

    [Fact]
    public void SeedAdmin_SoloExistenProfesionalesConRolProfesional_CreaElPrimerAdmin()
    {
        // Arrange
        using var context = CrearDbContext();
        var profesional = new Profesional
        {
            Id = 2,
            Nombre = "Carlos",
            Apellido = "Medico",
            Email = "carlos@meditool.test",
            Password = "hash",
            Rol = Rol.Profesional
        };
        context.Profesionales.Add(profesional);
        context.SaveChanges();

        var config = CrearConfiguracion("admin@meditool.test", "AdminPassword123!");

        // Act
        AdminSeeder.SeedAdmin(context, config);

        // Assert
        context.Profesionales.Should().HaveCount(2);
        context.Profesionales.Count(p => p.Rol == Rol.Admin).Should().Be(1);
        context.Profesionales.Count(p => p.Rol == Rol.Profesional).Should().Be(1);
    }
}
