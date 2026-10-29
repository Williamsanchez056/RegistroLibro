using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using RegistroLibro.Context;
using RegistroLibro.Models;

namespace RegistroLibro.Endpoints;

public static class EstudiantesEndpoints
{
    public static IEndpointRouteBuilder MapEstudiantesApi(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/estudiantes")
            .WithTags("Estudiantes");

        grupo.MapGet("/", async (IDbContextFactory<RegistroContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();

            var estudiantes = await db.Estudiantes
                .AsNoTracking()
                .OrderBy(e => e.Nombres)
                .ToListAsync();

            return Results.Ok(estudiantes);
        });

        grupo.MapGet("/{id:int}", async (int id, IDbContextFactory<RegistroContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();
            var estudiante = await db.Estudiantes
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EstudianteId == id);

            return estudiante is null
                ? Results.NotFound(new { mensaje = "No se encontro el estudiante." })
                : Results.Ok(estudiante);
        });

        grupo.MapPost("/", async (Estudiante estudiante, IDbContextFactory<RegistroContext> factory) =>
        {
            var errores = ValidarEstudiante(estudiante);
            if (errores.Count > 0)
                return Results.ValidationProblem(errores);

            await using var db = await factory.CreateDbContextAsync();

            estudiante.EstudianteId = 0;
            estudiante.Nombres = estudiante.Nombres.Trim();
            estudiante.Direccion = estudiante.Direccion.Trim();
            estudiante.Email = estudiante.Email.Trim();

            bool repetido = await db.Estudiantes.AnyAsync(e => e.Nombres == estudiante.Nombres);
            if (repetido)
                return Results.Conflict(new { mensaje = "Ya existe un estudiante con ese nombre." });

            db.Estudiantes.Add(estudiante);
            await db.SaveChangesAsync();

            return Results.Created($"/api/estudiantes/{estudiante.EstudianteId}", estudiante);
        });

        grupo.MapPut("/{id:int}", async (int id, Estudiante datos, IDbContextFactory<RegistroContext> factory) =>
        {
            var errores = ValidarEstudiante(datos);
            if (errores.Count > 0)
                return Results.ValidationProblem(errores);

            await using var db = await factory.CreateDbContextAsync();
            var estudiante = await db.Estudiantes.FindAsync(id);

            if (estudiante is null)
                return Results.NotFound(new { mensaje = "No se encontro el estudiante." });

            datos.Nombres = datos.Nombres.Trim();
            datos.Direccion = datos.Direccion.Trim();
            datos.Email = datos.Email.Trim();

            bool repetido = await db.Estudiantes.AnyAsync(e =>
                e.Nombres == datos.Nombres && e.EstudianteId != id);

            if (repetido)
                return Results.Conflict(new { mensaje = "Ya existe otro estudiante con ese nombre." });

            estudiante.Nombres = datos.Nombres;
            estudiante.Direccion = datos.Direccion;
            estudiante.Email = datos.Email;
            estudiante.FechaNacimiento = datos.FechaNacimiento;

            await db.SaveChangesAsync();

            return Results.Ok(estudiante);
        });

        grupo.MapDelete("/{id:int}", async (int id, IDbContextFactory<RegistroContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();
            var estudiante = await db.Estudiantes.FindAsync(id);

            if (estudiante is null)
                return Results.NotFound(new { mensaje = "No se encontro el estudiante." });

            bool tienePrestamos = await db.Prestamos.AnyAsync(p => p.Estudiante == estudiante.Nombres);
            if (tienePrestamos)
                return Results.Conflict(new { mensaje = "No se puede eliminar un estudiante que tiene prestamos registrados." });

            db.Estudiantes.Remove(estudiante);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        return app;
    }

    private static Dictionary<string, string[]> ValidarEstudiante(Estudiante estudiante)
    {
        var resultados = new List<ValidationResult>();
        var contexto = new ValidationContext(estudiante);
        Validator.TryValidateObject(estudiante, contexto, resultados, validateAllProperties: true);

        return resultados
            .SelectMany(error => error.MemberNames.Select(nombre => new
            {
                Nombre = nombre,
                Mensaje = error.ErrorMessage ?? "El dato no es valido."
            }))
            .GroupBy(error => error.Nombre)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Select(error => error.Mensaje).ToArray());
    }
}
