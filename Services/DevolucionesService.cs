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

            var prestamo = await contexto.Prestamos
                .FirstOrDefaultAsync(p => p.PrestamoId == devolucion.PrestamoId);

            if (prestamo is null)
                return Result.Failure(new Error("LOAN_NOT_FOUND", "No se encontro el prestamo."));

            if (devolucion.FechaDevolucion.Date < prestamo.FechaPrestamo.Date)
                return Result.Failure(new Error("INVALID_DATE", "La fecha de devolucion no puede ser anterior al prestamo."));

            var libro = await contexto.Libros
                .FirstOrDefaultAsync(l => l.Titulo == prestamo.Libro);

            if (libro is null)
                return Result.Failure(new Error("BOOK_NOT_FOUND", "No se encontro el libro asociado al prestamo."));

            if (devolucion.DevolucionId == 0)
            {
                if (prestamo.FechaEntrega.HasValue)
                    return Result.Failure(new Error("ALREADY_RETURNED", "Este prestamo ya fue devuelto."));

                bool yaExisteDevolucion = await contexto.Devoluciones
                    .AnyAsync(d => d.PrestamoId == devolucion.PrestamoId);

                if (yaExisteDevolucion)
                    return Result.Failure(new Error("ALREADY_RETURNED", "Este prestamo ya tiene una devolucion registrada."));

                prestamo.FechaEntrega = devolucion.FechaDevolucion;
                libro.CantidadDisponible++;

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

                if (existente.PrestamoId != devolucion.PrestamoId)
                    return Result.Failure(new Error("INVALID_RETURN", "No se puede cambiar el prestamo asociado a una devolucion."));

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

            var libro = await contexto.Libros
                .FirstOrDefaultAsync(l => l.Titulo == prestamo.Libro);

            if (libro is null)
                return Result.Failure(new Error("BOOK_NOT_FOUND", "No se encontro el libro asociado al prestamo."));

            if (libro.CantidadDisponible <= 0)
                return Result.Failure(new Error("OUT_OF_STOCK", "No se puede anular la devolucion porque no hay ejemplares disponibles para volver a descontar."));

            libro.CantidadDisponible--;
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

    private async Task<bool> Existe(int devolucionId)
    {
        await using var contexto = await _factory.CreateDbContextAsync();
        return await contexto.Devoluciones.AnyAsync(devolucion =>
            devolucion.DevolucionId == devolucionId);
    }

    private async Task<bool> Insertar(Devolucion devolucion)
    {
        await using var contexto = await _factory.CreateDbContextAsync();
        var prestamo = await contexto.Prestamos
            .FirstOrDefaultAsync(item => item.PrestamoId == devolucion.PrestamoId);

        if (prestamo is null || prestamo.FechaEntrega.HasValue)
            return false;

        var libro = await contexto.Libros
            .FirstOrDefaultAsync(item => item.Titulo == prestamo.Libro);

        if (libro is null)
            return false;

        prestamo.FechaEntrega = devolucion.FechaDevolucion;
        libro.CantidadDisponible++;
        contexto.Devoluciones.Add(devolucion);
        return await contexto.SaveChangesAsync() > 0;
    }

    private async Task<bool> Modificar(Devolucion devolucion)
    {
        await using var contexto = await _factory.CreateDbContextAsync();
        var existente = await contexto.Devoluciones
            .FirstOrDefaultAsync(item => item.DevolucionId == devolucion.DevolucionId);

        if (existente is null || existente.PrestamoId != devolucion.PrestamoId)
            return false;

        existente.FechaDevolucion = devolucion.FechaDevolucion;
        var prestamo = await contexto.Prestamos
            .FirstOrDefaultAsync(item => item.PrestamoId == existente.PrestamoId);

        if (prestamo is null)
            return false;

        prestamo.FechaEntrega = devolucion.FechaDevolucion;
        return await contexto.SaveChangesAsync() > 0;
    }

    private async Task<bool> AfectarPrestamo(RegistroContext contexto, int prestamoId, bool devuelto)
    {
        var prestamo = await contexto.Prestamos
            .FirstOrDefaultAsync(item => item.PrestamoId == prestamoId);

        if (prestamo is null)
            return false;

        if (devuelto)
        {
            if (prestamo.FechaEntrega.HasValue)
                return false;
            prestamo.FechaEntrega = DateTime.Today;
        }
        else
        {
            if (!prestamo.FechaEntrega.HasValue)
                return false;
            prestamo.FechaEntrega = null;
        }

        return true;
    }

}
