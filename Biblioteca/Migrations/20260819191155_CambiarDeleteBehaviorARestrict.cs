using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblioteca.Migrations
{
    /// <inheritdoc />
    public partial class CambiarDeleteBehaviorARestrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_asistencia_turno_turno_id",
                table: "asistencia");

            migrationBuilder.DropForeignKey(
                name: "FK_informe_paciente_paciente_id",
                table: "informe");

            migrationBuilder.DropForeignKey(
                name: "FK_paciente_profesional_paciente_paciente_id",
                table: "paciente_profesional");

            migrationBuilder.DropForeignKey(
                name: "FK_paciente_profesional_profesional_profesional_id",
                table: "paciente_profesional");

            migrationBuilder.DropForeignKey(
                name: "FK_reunion_profesional_profesional_id",
                table: "reunion");

            migrationBuilder.DropForeignKey(
                name: "FK_turno_paciente_paciente_id",
                table: "turno");

            migrationBuilder.DropForeignKey(
                name: "FK_turno_profesional_profesional_id",
                table: "turno");

            migrationBuilder.DropForeignKey(
                name: "FK_turno_turno_fijo_turno_fijo_id",
                table: "turno");

            migrationBuilder.DropForeignKey(
                name: "FK_turno_fijo_paciente_paciente_id",
                table: "turno_fijo");

            migrationBuilder.DropForeignKey(
                name: "FK_turno_fijo_profesional_profesional_id",
                table: "turno_fijo");

            migrationBuilder.AddForeignKey(
                name: "FK_asistencia_turno_turno_id",
                table: "asistencia",
                column: "turno_id",
                principalTable: "turno",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_informe_paciente_paciente_id",
                table: "informe",
                column: "paciente_id",
                principalTable: "paciente",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_paciente_profesional_paciente_paciente_id",
                table: "paciente_profesional",
                column: "paciente_id",
                principalTable: "paciente",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_paciente_profesional_profesional_profesional_id",
                table: "paciente_profesional",
                column: "profesional_id",
                principalTable: "profesional",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_reunion_profesional_profesional_id",
                table: "reunion",
                column: "profesional_id",
                principalTable: "profesional",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_turno_paciente_paciente_id",
                table: "turno",
                column: "paciente_id",
                principalTable: "paciente",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_turno_profesional_profesional_id",
                table: "turno",
                column: "profesional_id",
                principalTable: "profesional",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_turno_turno_fijo_turno_fijo_id",
                table: "turno",
                column: "turno_fijo_id",
                principalTable: "turno_fijo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_turno_fijo_paciente_paciente_id",
                table: "turno_fijo",
                column: "paciente_id",
                principalTable: "paciente",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_turno_fijo_profesional_profesional_id",
                table: "turno_fijo",
                column: "profesional_id",
                principalTable: "profesional",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_asistencia_turno_turno_id",
                table: "asistencia");

            migrationBuilder.DropForeignKey(
                name: "FK_informe_paciente_paciente_id",
                table: "informe");

            migrationBuilder.DropForeignKey(
                name: "FK_paciente_profesional_paciente_paciente_id",
                table: "paciente_profesional");

            migrationBuilder.DropForeignKey(
                name: "FK_paciente_profesional_profesional_profesional_id",
                table: "paciente_profesional");

            migrationBuilder.DropForeignKey(
                name: "FK_reunion_profesional_profesional_id",
                table: "reunion");

            migrationBuilder.DropForeignKey(
                name: "FK_turno_paciente_paciente_id",
                table: "turno");

            migrationBuilder.DropForeignKey(
                name: "FK_turno_profesional_profesional_id",
                table: "turno");

            migrationBuilder.DropForeignKey(
                name: "FK_turno_turno_fijo_turno_fijo_id",
                table: "turno");

            migrationBuilder.DropForeignKey(
                name: "FK_turno_fijo_paciente_paciente_id",
                table: "turno_fijo");

            migrationBuilder.DropForeignKey(
                name: "FK_turno_fijo_profesional_profesional_id",
                table: "turno_fijo");

            migrationBuilder.AddForeignKey(
                name: "FK_asistencia_turno_turno_id",
                table: "asistencia",
                column: "turno_id",
                principalTable: "turno",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_informe_paciente_paciente_id",
                table: "informe",
                column: "paciente_id",
                principalTable: "paciente",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_paciente_profesional_paciente_paciente_id",
                table: "paciente_profesional",
                column: "paciente_id",
                principalTable: "paciente",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_paciente_profesional_profesional_profesional_id",
                table: "paciente_profesional",
                column: "profesional_id",
                principalTable: "profesional",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reunion_profesional_profesional_id",
                table: "reunion",
                column: "profesional_id",
                principalTable: "profesional",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_turno_paciente_paciente_id",
                table: "turno",
                column: "paciente_id",
                principalTable: "paciente",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_turno_profesional_profesional_id",
                table: "turno",
                column: "profesional_id",
                principalTable: "profesional",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_turno_turno_fijo_turno_fijo_id",
                table: "turno",
                column: "turno_fijo_id",
                principalTable: "turno_fijo",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_turno_fijo_paciente_paciente_id",
                table: "turno_fijo",
                column: "paciente_id",
                principalTable: "paciente",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_turno_fijo_profesional_profesional_id",
                table: "turno_fijo",
                column: "profesional_id",
                principalTable: "profesional",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
