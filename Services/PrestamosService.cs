using Aplicada1.Core;
using Microsoft.EntityFrameworkCore;
using RegistroLibro.Context;
using RegistroLibro.Models;
using System.Linq.Expressions;

namespace RegistroLibro.Services;

public class PrestamosService : IService<Prestamo, int>
{
    private readonly IDbContextFactory<RegistroContext> _factory;

    public PrestamosService(IDbContextFactory<RegistroContext> factory)
    {
        _factory = factory;
    }

    public async Task<bool> Guardar(Prestamo prestamo)
    {
        if (string.IsNullOrWhiteSpace(prestamo.Libro) ||
            string.IsNullOrWhiteSpace(prestamo.Estudiante) ||
            prestamo.FechaDevolucion.Date < prestamo.FechaPrestamo.Date)
        {
            return false;
        }

        prestamo.Libro = prestamo.Libro.Trim();
        prestamo.Estudiante = prestamo.Estudiante.Trim();

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
                return false;

            existente.Libro = prestamo.Libro;
            existente.Estudiante = prestamo.Estudiante;
            existente.FechaPrestamo = prestamo.FechaPrestamo;
            existente.FechaDevolucion = prestamo.FechaDevolucion;
            existente.Observaciones = prestamo.Observaciones;
        }

        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<List<Prestamo>> Listar()
    {
        return await GetList(p => true);
    }

    public async Task<List<Prestamo>> GetList(Expression<Func<Prestamo, bool>> criterio)
    {
        await using var contexto = await _factory.CreateDbContextAsync();

        return await contexto.Prestamos
            .Where(criterio)
            .AsNoTracking()
            .OrderByDescending(p => p.FechaPrestamo)
            .ToListAsync();
    }

    public async Task<Prestamo?> Buscar(int id)
    {
        await using var contexto = await _factory.CreateDbContextAsync();

        return await contexto.Prestamos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PrestamoId == id);
    }

    public async Task<bool> Eliminar(int id)
    {
        await using var contexto = await _factory.CreateDbContextAsync();

        var prestamo = await contexto.Prestamos.FindAsync(id);
        if (prestamo is null)
            return false;

        var tieneDevolucion = await contexto.Devoluciones
            .AnyAsync(d => d.PrestamoId == id);

        if (tieneDevolucion)
            return false;

        contexto.Prestamos.Remove(prestamo);
        return await contexto.SaveChangesAsync() > 0;
    }
}
