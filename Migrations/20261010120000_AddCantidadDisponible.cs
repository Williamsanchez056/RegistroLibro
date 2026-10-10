using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistroLibro.Migrations
{
    public partial class AddCantidadDisponible : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CantidadDisponible",
                table: "Libro",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CantidadDisponible",
                table: "Libro");
        }
    }
}
