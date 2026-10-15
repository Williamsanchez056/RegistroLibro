using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RegistroLibro.Context;

#nullable disable

namespace RegistroLibro.Migrations;

[DbContext(typeof(RegistroContext))]
[Migration("20261015030000_QuitarConceptoEntrada")]
public partial class QuitarConceptoEntrada : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Concepto",
            table: "Entradas");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Concepto",
            table: "Entradas",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");
    }
}
