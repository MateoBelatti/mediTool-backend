using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Biblioteca.Migrations
{
    /// <inheritdoc />
    public partial class EliminarAsistenciaMoverATurno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asistencia");

            migrationBuilder.AddColumn<bool>(
                name: "facturable",
                table: "turno",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_registro",
                table: "turno",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "justificada",
                table: "turno",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "observaciones",
                table: "turno",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "facturable",
                table: "turno");

            migrationBuilder.DropColumn(
                name: "fecha_registro",
                table: "turno");

            migrationBuilder.DropColumn(
                name: "justificada",
                table: "turno");

            migrationBuilder.DropColumn(
                name: "observaciones",
                table: "turno");

            migrationBuilder.CreateTable(
                name: "asistencia",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    turno_id = table.Column<int>(type: "integer", nullable: true),
                    asistio = table.Column<bool>(type: "boolean", nullable: false),
                    facturable = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    justificada = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asistencia", x => x.id);
                    table.ForeignKey(
                        name: "FK_asistencia_turno_turno_id",
                        column: x => x.turno_id,
                        principalTable: "turno",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_asistencia_turno_id",
                table: "asistencia",
                column: "turno_id");
        }
    }
}
