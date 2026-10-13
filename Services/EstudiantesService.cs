using Aplicada1.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RegistroLibro.Context;
using RegistroLibro.Models;
using System.Linq.Expressions;

namespace RegistroLibro.Services;

public class EstudiantesService : IServiceResult<Estudiante, int>
{
    private readonly IDbContextFactory<RegistroContext> _factory;

    public EstudiantesService(IDbContextFactory<RegistroContext> factory)
    {
        _factory = factory;
    }

    public async Task<Result> Guardar(Estudiante estudiante)
    {
        if (string.IsNullOrWhiteSpace(estudiante.Nombres))
            return Result.Failure(new Error("VALIDATION", "El nombre del estudiante es obligatorio."));

        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();

            estudiante.Nombres = estudiante.Nombres.Trim();
            estudiante.Direccion = estudiante.Direccion.Trim();
            estudiante.Email = estudiante.Email.Trim();

            bool repetido = await contexto.Estudiantes.AnyAsync(e =>
                e.Nombres == estudiante.Nombres &&
                e.EstudianteId != estudiante.EstudianteId);

            if (repetido)
                return Result.Failure(new Error("DUPLICATE_NAME", "Ya existe un estudiante con ese nombre."));

            if (estudiante.EstudianteId == 0)
                contexto.Estudiantes.Add(estudiante);
            else
            {
                var existente = await contexto.Estudiantes.FindAsync(estudiante.EstudianteId);
                if (existente is null)
                    return Result.Failure(new Error("NOT_FOUND", "No se encontro el estudiante."));

                existente.Nombres = estudiante.Nombres;
                existente.Direccion = estudiante.Direccion;
                existente.Email = estudiante.Email;
                existente.FechaNacimiento = estudiante.FechaNacimiento;
            }

            await contexto.SaveChangesAsync();
            return Result.Success();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is SqlException sql &&
            (sql.Number == 2601 || sql.Number == 2627))
        {
            return Result.Failure(new Error("DUPLICATE_NAME", "Ya existe un estudiante con ese nombre."));
        }
        catch
        {
            return Result.Failure(new Error("DATABASE_ERROR", "No se pudo guardar el estudiante."));
        }
    }

    public async Task<Result<List<Estudiante>>> GetList(Expression<Func<Estudiante, bool>> criterio)
    {
        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();
            var estudiantes = await contexto.Estudiantes
                .Where(criterio)
                .AsNoTracking()
                .OrderBy(e => e.Nombres)
                .ToListAsync();

            return Result.Success(estudiantes);
        }
        catch
        {
            return Result.Failure<List<Estudiante>>(new Error("DATABASE_ERROR", "No se pudieron consultar los estudiantes."));
        }
    }

    public async Task<Result<Estudiante?>> Buscar(int id)
    {
        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();
            var estudiante = await contexto.Estudiantes
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EstudianteId == id);

            return estudiante is null
                ? Result.Failure<Estudiante?>(new Error("NOT_FOUND", "No se encontro el estudiante."))
                : Result.Success<Estudiante?>(estudiante);
        }
        catch
        {
            return Result.Failure<Estudiante?>(new Error("DATABASE_ERROR", "No se pudo consultar el estudiante."));
        }
    }

    public async Task<Result> Eliminar(int id)
    {
        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();
            var estudiante = await contexto.Estudiantes.FindAsync(id);

            if (estudiante is null)
                return Result.Failure(new Error("NOT_FOUND", "No se encontro el estudiante."));

            var tienePrestamos = await contexto.Prestamos
                .AnyAsync(p => p.Estudiante == estudiante.Nombres);

            if (tienePrestamos)
                return Result.Failure(new Error("LOAN_EXISTS", "No se puede eliminar un estudiante que tiene prestamos registrados."));

            contexto.Estudiantes.Remove(estudiante);
            await contexto.SaveChangesAsync();
            return Result.Success();
        }
        catch
        {
            return Result.Failure(new Error("DATABASE_ERROR", "No se pudo eliminar el estudiante."));
        }
    }

    public async Task<Result<bool>> ExisteNombre(string nombres, int estudianteId = 0)
    {
        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();
            var nombre = nombres.Trim();
            var existe = await contexto.Estudiantes.AnyAsync(e =>
                e.Nombres == nombre &&
                e.EstudianteId != estudianteId);

            return Result.Success(existe);
        }
        catch
        {
            return Result.Failure<bool>(new Error("DATABASE_ERROR", "No se pudo validar el nombre del estudiante."));
        }
    }
}