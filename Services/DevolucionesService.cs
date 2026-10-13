using Aplicada1.Core;
using Microsoft.EntityFrameworkCore;
using RegistroLibro.Context;
using RegistroLibro.Models;
using System.Linq.Expressions;

namespace RegistroLibro.Services;

public class DevolucionesService : IService<Devolucion, int>
{
    private readonly IDbContextFactory<RegistroContext> _factory;

    public DevolucionesService(IDbContextFactory<RegistroContext> factory)
    {
        _factory = factory;
    }

    public async Task<bool> Guardar(Devolucion devolucion)
    {
        await using var contexto = await _factory.CreateDbContextAsync();

        if (devolucion.DevolucionId == 0)
        {
            var prestamo = await contexto.Prestamos
                .FirstOrDefaultAsync(p => p.PrestamoId == devolucion.PrestamoId);

            if (prestamo is null || prestamo.FechaEntrega.HasValue)
                return false;

            if (devolucion.FechaDevolucion.Date < prestamo.FechaPrestamo.Date)
                return false;

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
                return false;

            var prestamo = await contexto.Prestamos
                .FirstOrDefaultAsync(p => p.PrestamoId == existente.PrestamoId);

            if (prestamo is null ||
                devolucion.PrestamoId != existente.PrestamoId ||
                devolucion.FechaDevolucion.Date < prestamo.FechaPrestamo.Date)
            {
                return false;
            }

            existente.FechaDevolucion = devolucion.FechaDevolucion;
            prestamo.FechaEntrega = devolucion.FechaDevolucion;
        }

        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<List<Devolucion>> Listar()
    {
        return await GetList(d => true);
    }

    public async Task<List<Devolucion>> GetList(Expression<Func<Devolucion, bool>> criterio)
    {
        await using var contexto = await _factory.CreateDbContextAsync();

        return await contexto.Devoluciones
            .Include(d => d.Prestamo)
            .Where(criterio)
            .AsNoTracking()
            .OrderByDescending(d => d.FechaDevolucion)
            .ToListAsync();
    }

    public async Task<Devolucion?> Buscar(int id)
    {
        await using var contexto = await _factory.CreateDbContextAsync();

        return await contexto.Devoluciones
            .Include(d => d.Prestamo)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DevolucionId == id);
    }

    public async Task<bool> Eliminar(int id)
    {
        await using var contexto = await _factory.CreateDbContextAsync();

        var devolucion = await contexto.Devoluciones
            .FirstOrDefaultAsync(d => d.DevolucionId == id);

        if (devolucion is null)
            return false;

        var prestamo = await contexto.Prestamos
            .FirstOrDefaultAsync(p => p.PrestamoId == devolucion.PrestamoId);

        if (prestamo is null)
            return false;

        prestamo.FechaEntrega = null;
        contexto.Devoluciones.Remove(devolucion);

        return await contexto.SaveChangesAsync() > 0;
    }
}
