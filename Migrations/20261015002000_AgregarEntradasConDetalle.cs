using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RegistroLibro.Context;

#nullable disable

namespace RegistroLibro.Migrations;

[DbContext(typeof(RegistroContext))]
[Migration("20261015002000_AgregarEntradasConDetalle")]
public partial class AgregarEntradasConDetalle : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Entradas",
            columns: table => new
            {
                EntradaId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                Total = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Entradas", x => x.EntradaId);
            });

        migrationBuilder.CreateTable(
            name: "EntradasDetalle",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                EntradaId = table.Column<int>(type: "int", nullable: false),
                LibroId = table.Column<int>(type: "int", nullable: false),
                Cantidad = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EntradasDetalle", x => x.Id);
                table.ForeignKey(
                    name: "FK_EntradasDetalle_Entradas_EntradaId",
                    column: x => x.EntradaId,
                    principalTable: "Entradas",
                    principalColumn: "EntradaId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_EntradasDetalle_Libro_LibroId",
                    column: x => x.LibroId,
                    principalTable: "Libro",
                    principalColumn: "LibroId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_EntradasDetalle_EntradaId",
            table: "EntradasDetalle",
            column: "EntradaId");

        migrationBuilder.CreateIndex(
            name: "IX_EntradasDetalle_LibroId",
            table: "EntradasDetalle",
            column: "LibroId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "EntradasDetalle");
        migrationBuilder.DropTable(name: "Entradas");
    }
}
