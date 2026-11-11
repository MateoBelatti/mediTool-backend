using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblioteca.Migrations
{
    /// <inheritdoc />
    public partial class AgregarProfesionalesAReunion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reunion_profesional",
                columns: table => new
                {
                    reunion_id = table.Column<int>(type: "integer", nullable: false),
                    profesional_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reunion_profesional", x => new { x.reunion_id, x.profesional_id });
                    table.ForeignKey(
                        name: "FK_reunion_profesional_profesional_profesional_id",
                        column: x => x.profesional_id,
                        principalTable: "profesional",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reunion_profesional_reunion_reunion_id",
                        column: x => x.reunion_id,
                        principalTable: "reunion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reunion_profesional_profesional_id",
                table: "reunion_profesional",
                column: "profesional_id");

            migrationBuilder.Sql(@"
                INSERT INTO reunion_profesional (reunion_id, profesional_id)
                SELECT id, profesional_id FROM reunion
                ON CONFLICT DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reunion_profesional");
        }
    }
}
