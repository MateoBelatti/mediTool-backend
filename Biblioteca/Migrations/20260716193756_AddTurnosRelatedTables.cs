using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Biblioteca.Migrations
{
    /// <inheritdoc />
    public partial class AddTurnosRelatedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "informe",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    paciente_id = table.Column<int>(type: "integer", nullable: true),
                    profesional_id = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    contenido = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_informe", x => x.id);
                    table.ForeignKey(
                        name: "FK_informe_paciente_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_informe_profesional_profesional_id",
                        column: x => x.profesional_id,
                        principalTable: "profesional",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reunion",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    titulo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    fecha_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modalidad = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    profesional_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reunion", x => x.id);
                    table.ForeignKey(
                        name: "FK_reunion_profesional_profesional_id",
                        column: x => x.profesional_id,
                        principalTable: "profesional",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "turno_fijo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    paciente_id = table.Column<int>(type: "integer", nullable: false),
                    profesional_id = table.Column<int>(type: "integer", nullable: false),
                    dia_semana = table.Column<int>(type: "integer", nullable: false),
                    hora = table.Column<TimeSpan>(type: "interval", nullable: false),
                    duracion_min = table.Column<int>(type: "integer", nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateOnly>(type: "date", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_turno_fijo", x => x.id);
                    table.ForeignKey(
                        name: "FK_turno_fijo_paciente_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_turno_fijo_profesional_profesional_id",
                        column: x => x.profesional_id,
                        principalTable: "profesional",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "turno",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    turno_fijo_id = table.Column<int>(type: "integer", nullable: true),
                    paciente_id = table.Column<int>(type: "integer", nullable: false),
                    profesional_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    duracion_min = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_turno", x => x.id);
                    table.ForeignKey(
                        name: "FK_turno_paciente_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "paciente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_turno_profesional_profesional_id",
                        column: x => x.profesional_id,
                        principalTable: "profesional",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_turno_turno_fijo_turno_fijo_id",
                        column: x => x.turno_fijo_id,
                        principalTable: "turno_fijo",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "asistencia",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    turno_id = table.Column<int>(type: "integer", nullable: true),
                    asistio = table.Column<bool>(type: "boolean", nullable: false),
                    justificada = table.Column<bool>(type: "boolean", nullable: false),
                    facturable = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asistencia", x => x.id);
                    table.ForeignKey(
                        name: "FK_asistencia_turno_turno_id",
                        column: x => x.turno_id,
                        principalTable: "turno",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_asistencia_turno_id",
                table: "asistencia",
                column: "turno_id");

            migrationBuilder.CreateIndex(
                name: "IX_informe_paciente_id",
                table: "informe",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "IX_informe_profesional_id",
                table: "informe",
                column: "profesional_id");

            migrationBuilder.CreateIndex(
                name: "IX_reunion_profesional_id",
                table: "reunion",
                column: "profesional_id");

            migrationBuilder.CreateIndex(
                name: "IX_turno_paciente_id",
                table: "turno",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "IX_turno_profesional_id",
                table: "turno",
                column: "profesional_id");

            migrationBuilder.CreateIndex(
                name: "IX_turno_turno_fijo_id",
                table: "turno",
                column: "turno_fijo_id");

            migrationBuilder.CreateIndex(
                name: "IX_turno_fijo_paciente_id",
                table: "turno_fijo",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "IX_turno_fijo_profesional_id",
                table: "turno_fijo",
                column: "profesional_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asistencia");

            migrationBuilder.DropTable(
                name: "informe");

            migrationBuilder.DropTable(
                name: "reunion");

            migrationBuilder.DropTable(
                name: "turno");

            migrationBuilder.DropTable(
                name: "turno_fijo");
        }
    }
}
