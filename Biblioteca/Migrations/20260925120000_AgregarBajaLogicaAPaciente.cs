using System;
using Biblioteca.Repository;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblioteca.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260925120000_AgregarBajaLogicaAPaciente")]
    public partial class AgregarBajaLogicaAPaciente : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "activo",
                table: "paciente",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_baja",
                table: "paciente",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_baja",
                table: "paciente");

            migrationBuilder.DropColumn(
                name: "activo",
                table: "paciente");
        }
    }
}
