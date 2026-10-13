using Aplicada1.Core;
using Microsoft.EntityFrameworkCore;
using RegistroLibro.Context;
using RegistroLibro.Models;
using System.Linq.Expressions;

namespace RegistroLibro.Services;

public class DevolucionesService : IServiceResult<Devolucion, int>
{
    private readonly IDbContextFactory<RegistroContext> _factory;

    public DevolucionesService(IDbContextFactory<RegistroContext> factory)
    {
        _factory = factory;
    }

    public async Task<Result> Guardar(Devolucion devolucion)
    {
        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();

            if (devolucion.DevolucionId == 0)
            {
                var prestamo = await contexto.Prestamos
                    .FirstOrDefaultAsync(p => p.PrestamoId == devolucion.PrestamoId);

                if (prestamo is null)
                    return Result.Failure(new Error("LOAN_NOT_FOUND", "No se encontro el prestamo."));

                if (prestamo.FechaEntrega.HasValue)
                    return Result.Failure(new Error("ALREADY_RETURNED", "Este prestamo ya fue devuelto."));

                if (devolucion.FechaDevolucion.Date < prestamo.FechaPrestamo.Date)
                    return Result.Failure(new Error("INVALID_DATE", "La fecha de devolucion no puede ser anterior al prestamo."));

                prestamo.FechaEntrega = devolucion.FechaDevolucion;
                contexto.Devoluciones.Add(new Devolucion
                {
                    PrestamoId = devolucion.PrestamoId,
                    FechaDevolucion = devolucion.FechaDevolucion
                });
            }
            else
            {
                var existente = await contexto.Devoluciones
                    .FirstOrDefaultAsync(d => d.DevolucionId == devolucion.DevolucionId);

                if (existente is null)
                    return Result.Failure(new Error("NOT_FOUND", "No se encontro la devolucion."));

                var prestamo = await contexto.Prestamos
                    .FirstOrDefaultAsync(p => p.PrestamoId == existente.PrestamoId);

                if (prestamo is null ||
                    devolucion.PrestamoId != existente.PrestamoId ||
                    devolucion.FechaDevolucion.Date < prestamo.FechaPrestamo.Date)
                {
                    return Result.Failure(new Error("INVALID_RETURN", "Los datos de la devolucion no son validos."));
                }

                existente.FechaDevolucion = devolucion.FechaDevolucion;
                prestamo.FechaEntrega = devolucion.FechaDevolucion;
            }

            await contexto.SaveChangesAsync();
            return Result.Success();
        }
        catch
        {
            return Result.Failure(new Error("DATABASE_ERROR", "No se pudo guardar la devolucion."));
        }
    }

    public async Task<Result<List<Devolucion>>> GetList(Expression<Func<Devolucion, bool>> criterio)
    {
        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();
            var devoluciones = await contexto.Devoluciones
                .Include(d => d.Prestamo)
                .Where(criterio)
                .AsNoTracking()
                .OrderByDescending(d => d.FechaDevolucion)
                .ToListAsync();

            return Result.Success(devoluciones);
        }
        catch
        {
            return Result.Failure<List<Devolucion>>(new Error("DATABASE_ERROR", "No se pudieron consultar las devoluciones."));
        }
    }

    public async Task<Result<Devolucion?>> Buscar(int id)
    {
        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();
            var devolucion = await contexto.Devoluciones
                .Include(d => d.Prestamo)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DevolucionId == id);

            return devolucion is null
                ? Result.Failure<Devolucion?>(new Error("NOT_FOUND", "No se encontro la devolucion."))
                : Result.Success<Devolucion?>(devolucion);
        }
        catch
        {
            return Result.Failure<Devolucion?>(new Error("DATABASE_ERROR", "No se pudo consultar la devolucion."));
        }
    }

    public async Task<Result> Eliminar(int id)
    {
        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();
            var devolucion = await contexto.Devoluciones
                .FirstOrDefaultAsync(d => d.DevolucionId == id);

            if (devolucion is null)
                return Result.Failure(new Error("NOT_FOUND", "No se encontro la devolucion."));

            var prestamo = await contexto.Prestamos
                .FirstOrDefaultAsync(p => p.PrestamoId == devolucion.PrestamoId);

            if (prestamo is null)
                return Result.Failure(new Error("LOAN_NOT_FOUND", "No se encontro el prestamo asociado."));

            prestamo.FechaEntrega = null;
            contexto.Devoluciones.Remove(devolucion);
            await contexto.SaveChangesAsync();

            return Result.Success();
        }
        catch
        {
            return Result.Failure(new Error("DATABASE_ERROR", "No se pudo eliminar la devolucion."));
        }
    }
}