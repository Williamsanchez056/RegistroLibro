using Microsoft.EntityFrameworkCore;
using RegistroLibro.Context;
using RegistroLibro.Models;
using System.Linq.Expressions;

namespace RegistroLibro.Services;

public class PrestamoService
{
    private readonly IDbContextFactory<RegistroContext> _factory;

    public PrestamoService(IDbContextFactory<RegistroContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<Prestamo>> Listar()
    {
        return await GetList(p => true);
    }

    public async Task<List<Prestamo>> GetList(Expression<Func<Prestamo, bool>> criterio)
    {
        using var contexto = await _factory.CreateDbContextAsync();

        return await contexto.Prestamos
            .Where(criterio)
            .AsNoTracking()
            .OrderByDescending(p => p.FechaPrestamo)
            .ToListAsync();
    }

    public async Task<Prestamo?> Buscar(int id)
    {
        using var contexto = await _factory.CreateDbContextAsync();

        return await contexto.Prestamos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PrestamoId == id);
    }

    public async Task<bool> Guardar(Prestamo prestamo)
    {
        using var contexto = await _factory.CreateDbContextAsync();

        if (prestamo.PrestamoId == 0)
        {
            contexto.Prestamos.Add(prestamo);
        }
        else
        {
            contexto.Prestamos.Update(prestamo);
        }

        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<bool> Eliminar(int id)
    {
        using var contexto = await _factory.CreateDbContextAsync();

        var prestamo = await contexto.Prestamos.FindAsync(id);

        if (prestamo is null)
            return false;

        contexto.Prestamos.Remove(prestamo);

        return await contexto.SaveChangesAsync() > 0;
    }
}