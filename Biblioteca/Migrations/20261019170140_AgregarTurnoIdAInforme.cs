using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblioteca.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTurnoIdAInforme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "turno_id",
                table: "informe",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_informe_turno_id",
                table: "informe",
                column: "turno_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_informe_turno_turno_id",
                table: "informe",
                column: "turno_id",
                principalTable: "turno",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_informe_turno_turno_id",
                table: "informe");

            migrationBuilder.DropIndex(
                name: "IX_informe_turno_id",
                table: "informe");

            migrationBuilder.DropColumn(
                name: "turno_id",
                table: "informe");
        }
    }
}
