using Biblioteca.Entities;
using Biblioteca.Repository;
using Microsoft.Extensions.Configuration;
using Utils.Enums;
using BCrypt.Net;
using System.Linq;

namespace mediTool.Seeders
{
    public static class AdminSeeder
    {
        public static void SeedAdmin(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            var adminEmail = configuration["Seed:AdminEmail"];
            var adminPassword = configuration["Seed:AdminPassword"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                // Si no hay variables de entorno, no sembramos nada.
                return;
            }

            // Chequeamos si ya existe algún administrador
            if (!dbContext.Profesionales.Any(p => p.Rol == Rol.Admin))
            {
                var admin = new Profesional
                {
                    Nombre = "Admin",
                    Apellido = "Admin",
                    Email = adminEmail,
                    Password = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                    Rol = Rol.Admin
                };

                dbContext.Profesionales.Add(admin);
                dbContext.SaveChanges();
            }
        }
    }
}
