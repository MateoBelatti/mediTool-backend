using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblioteca.Migrations
{
    /// <inheritdoc />
    public partial class MoverRelacionInformeATurno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "informe_id",
                table: "turno",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE turno t
                  SET informe_id = i.id
                  FROM informe i
                  WHERE i.turno_id = t.id;");

            migrationBuilder.DropForeignKey(
                name: "FK_informe_turno_turno_id",
                table: "informe");

            migrationBuilder.DropIndex(
                name: "IX_informe_turno_id",
                table: "informe");

            migrationBuilder.DropColumn(
                name: "turno_id",
                table: "informe");

            migrationBuilder.CreateIndex(
                name: "IX_turno_informe_id",
                table: "turno",
                column: "informe_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_turno_informe_informe_id",
                table: "turno",
                column: "informe_id",
                principalTable: "informe",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_turno_informe_informe_id",
                table: "turno");

            migrationBuilder.DropIndex(
                name: "IX_turno_informe_id",
                table: "turno");

            migrationBuilder.AddColumn<int>(
                name: "turno_id",
                table: "informe",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE informe i
                  SET turno_id = t.id
                  FROM turno t
                  WHERE t.informe_id = i.id;");

            migrationBuilder.DropColumn(
                name: "informe_id",
                table: "turno");

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

    }
}
