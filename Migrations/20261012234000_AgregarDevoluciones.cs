using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RegistroLibro.Context;

#nullable disable

namespace RegistroLibro.Migrations;

[DbContext(typeof(RegistroContext))]
[Migration("20261012234000_AgregarDevoluciones")]
public partial class AgregarDevoluciones : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Devoluciones",
            columns: table => new
            {
                DevolucionId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                PrestamoId = table.Column<int>(type: "int", nullable: false),
                FechaDevolucion = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Devoluciones", x => x.DevolucionId);
                table.ForeignKey(
                    name: "FK_Devoluciones_Prestamos_PrestamoId",
                    column: x => x.PrestamoId,
                    principalTable: "Prestamos",
                    principalColumn: "PrestamoId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Devoluciones_PrestamoId",
            table: "Devoluciones",
            column: "PrestamoId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Devoluciones");
    }
}
