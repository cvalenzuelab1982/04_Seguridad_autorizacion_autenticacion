using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Seguridad_autorizacion_autenticacion.Migrations
{
    public partial class FechaPublicacionLibro : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fechaPublicacion",
                table: "Libros",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fechaPublicacion",
                table: "Libros");
        }
    }
}
