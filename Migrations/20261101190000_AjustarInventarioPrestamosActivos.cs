using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RegistroLibro.Context;

#nullable disable

namespace RegistroLibro.Migrations;

[DbContext(typeof(RegistroContext))]
[Migration("20261101190000_AjustarInventarioPrestamosActivos")]
public partial class AjustarInventarioPrestamosActivos : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE l
            SET l.CantidadDisponible =
                CASE
                    WHEN l.CantidadDisponible >= p.Pendientes
                        THEN l.CantidadDisponible - p.Pendientes
                    ELSE 0
                END
            FROM Libro AS l
            INNER JOIN
            (
                SELECT LTRIM(RTRIM(Libro)) AS Titulo, COUNT(*) AS Pendientes
                FROM Prestamos
                WHERE FechaEntrega IS NULL
                GROUP BY LTRIM(RTRIM(Libro))
            ) AS p
                ON LTRIM(RTRIM(l.Titulo)) = p.Titulo;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE l
            SET l.CantidadDisponible = l.CantidadDisponible + p.Pendientes
            FROM Libro AS l
            INNER JOIN
            (
                SELECT LTRIM(RTRIM(Libro)) AS Titulo, COUNT(*) AS Pendientes
                FROM Prestamos
                WHERE FechaEntrega IS NULL
                GROUP BY LTRIM(RTRIM(Libro))
            ) AS p
                ON LTRIM(RTRIM(l.Titulo)) = p.Titulo;
            """);
    }
}