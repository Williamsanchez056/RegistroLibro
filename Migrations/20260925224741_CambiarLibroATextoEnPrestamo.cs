using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistroLibro.Migrations
{
    /// <inheritdoc />
    public partial class CambiarLibroATextoEnPrestamo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prestamos_Libro_LibroId",
                table: "Prestamos");

            migrationBuilder.DropIndex(
                name: "IX_Prestamos_LibroId",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "LibroId",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "Prestamos");

            migrationBuilder.AddColumn<string>(
                name: "Libro",
                table: "Prestamos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Libro",
                table: "Prestamos");

            migrationBuilder.AddColumn<int>(
                name: "LibroId",
                table: "Prestamos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "Prestamos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prestamos_LibroId",
                table: "Prestamos",
                column: "LibroId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prestamos_Libro_LibroId",
                table: "Prestamos",
                column: "LibroId",
                principalTable: "Libro",
                principalColumn: "LibroId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
