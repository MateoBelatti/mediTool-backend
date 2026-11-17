using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Biblioteca.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDocumentosPaciente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "archivo",
                columns: table => new
                {
                    clave = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    contenido = table.Column<byte[]>(type: "bytea", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_archivo", x => x.clave);
                });

            migrationBuilder.CreateTable(
                name: "documento_paciente",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    paciente_id = table.Column<int>(type: "integer", nullable: false),
                    profesional_id = table.Column<int>(type: "integer", nullable: false),
                    archivo_clave = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    nombre_original = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    extension = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    content_type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    tamano_bytes = table.Column<long>(type: "bigint", nullable: false),
                    fecha_subida = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documento_paciente", x => x.id);
                    table.ForeignKey(
                        name: "FK_documento_paciente_archivo_archivo_clave",
                        column: x => x.archivo_clave,
                        principalTable: "archivo",
                        principalColumn: "clave",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_documento_paciente_paciente_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_documento_paciente_profesional_profesional_id",
                        column: x => x.profesional_id,
                        principalTable: "profesional",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_documento_paciente_archivo_clave",
                table: "documento_paciente",
                column: "archivo_clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_documento_paciente_paciente_id",
                table: "documento_paciente",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "IX_documento_paciente_profesional_id",
                table: "documento_paciente",
                column: "profesional_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documento_paciente");

            migrationBuilder.DropTable(
                name: "archivo");
        }
    }
}
