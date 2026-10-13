using Aplicada1.Core;
using Microsoft.EntityFrameworkCore;
using RegistroLibro.Context;
using RegistroLibro.Models;
using System.Linq.Expressions;

namespace RegistroLibro.Services;

public class PrestamosService : IServiceResult<Prestamo, int>
{
    private readonly IDbContextFactory<RegistroContext> _factory;

    public PrestamosService(IDbContextFactory<RegistroContext> factory)
    {
        _factory = factory;
    }

    public async Task<Result> Guardar(Prestamo prestamo)
    {
        if (string.IsNullOrWhiteSpace(prestamo.Libro) ||
            string.IsNullOrWhiteSpace(prestamo.Estudiante) ||
            prestamo.FechaDevolucion.Date < prestamo.FechaPrestamo.Date)
        {
            return Result.Failure(new Error("VALIDATION", "El libro, el estudiante y un rango de fechas valido son obligatorios."));
        }

        prestamo.Libro = prestamo.Libro.Trim();
        prestamo.Estudiante = prestamo.Estudiante.Trim();

        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();

            if (prestamo.PrestamoId == 0)
            {
                contexto.Prestamos.Add(prestamo);
            }
            else
            {
                var existente = await contexto.Prestamos
                    .FirstOrDefaultAsync(p => p.PrestamoId == prestamo.PrestamoId);

                if (existente is null)
                    return Result.Failure(new Error("NOT_FOUND", "No se encontro el prestamo."));

                existente.Libro = prestamo.Libro;
                existente.Estudiante = prestamo.Estudiante;
                existente.FechaPrestamo = prestamo.FechaPrestamo;
                existente.FechaDevolucion = prestamo.FechaDevolucion;
                existente.Observaciones = prestamo.Observaciones;
            }

            await contexto.SaveChangesAsync();
            return Result.Success();
        }
        catch
        {
            return Result.Failure(new Error("DATABASE_ERROR", "No se pudo guardar el prestamo."));
        }
    }

    public async Task<Result<List<Prestamo>>> GetList(Expression<Func<Prestamo, bool>> criterio)
    {
        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();
            var prestamos = await contexto.Prestamos
                .Where(criterio)
                .AsNoTracking()
                .OrderByDescending(p => p.FechaPrestamo)
                .ToListAsync();

            return Result.Success(prestamos);
        }
        catch
        {
            return Result.Failure<List<Prestamo>>(new Error("DATABASE_ERROR", "No se pudieron consultar los prestamos."));
        }
    }

    public async Task<Result<Prestamo?>> Buscar(int id)
    {
        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();
            var prestamo = await contexto.Prestamos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PrestamoId == id);

            return prestamo is null
                ? Result.Failure<Prestamo?>(new Error("NOT_FOUND", "No se encontro el prestamo."))
                : Result.Success<Prestamo?>(prestamo);
        }
        catch
        {
            return Result.Failure<Prestamo?>(new Error("DATABASE_ERROR", "No se pudo consultar el prestamo."));
        }
    }

    public async Task<Result> Eliminar(int id)
    {
        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();
            var prestamo = await contexto.Prestamos.FindAsync(id);

            if (prestamo is null)
                return Result.Failure(new Error("NOT_FOUND", "No se encontro el prestamo."));

            var tieneDevolucion = await contexto.Devoluciones.AnyAsync(d => d.PrestamoId == id);
            if (tieneDevolucion)
                return Result.Failure(new Error("RETURN_EXISTS", "No se puede eliminar un prestamo que tiene una devolucion registrada."));

            contexto.Prestamos.Remove(prestamo);
            await contexto.SaveChangesAsync();
            return Result.Success();
        }
        catch
        {
            return Result.Failure(new Error("DATABASE_ERROR", "No se pudo eliminar el prestamo."));
        }
    }
}