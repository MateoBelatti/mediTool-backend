using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblioteca.Migrations
{
    /// <inheritdoc />
    public partial class CorreccionZonaHoraria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE turno SET fecha_hora = fecha_hora + interval '3 hours';");
            migrationBuilder.Sql("UPDATE reunion SET fecha_hora = fecha_hora + interval '3 hours';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE turno SET fecha_hora = fecha_hora - interval '3 hours';");
            migrationBuilder.Sql("UPDATE reunion SET fecha_hora = fecha_hora - interval '3 hours';");
        }
    }
}
