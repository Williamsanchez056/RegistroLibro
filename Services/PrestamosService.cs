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
        using var contexto = await _factory.CreateDbContextAsync();
        if (prestamo.PrestamoId == 0)
        {
            // Nuevo préstamo: decrementar cantidad disponible del libro
            var libro = await contexto.Libros
                .FirstOrDefaultAsync(l => l.Titulo == prestamo.Libro);

            if (libro != null)
            {
                if (libro.CantidadDisponible <= 0)
                    return false; // No hay libros disponibles
                libro.CantidadDisponible--;
                contexto.Libros.Update(libro);
            }

            contexto.Prestamos.Add(prestamo);
        }
        else
            contexto.Prestamos.Update(prestamo);

        return await contexto.SaveChangesAsync() > 0;
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
    public async Task<bool> Eliminar(int id)
    {
        using var contexto = await _factory.CreateDbContextAsync();
        var prestamo = await contexto.Prestamos.FindAsync(id);
        if (prestamo is null)
            return false;
        contexto.Prestamos.Remove(prestamo);
        return await contexto.SaveChangesAsync() > 0;
    }

    // Elimina un préstamo e incrementa CantidadDisponible del libro
    public async Task<bool> Devolver(int id)
    {
        using var contexto = await _factory.CreateDbContextAsync();
        var prestamo = await contexto.Prestamos.FindAsync(id);
        if (prestamo is null)
            return false;

        // Incrementar cantidad disponible del libro
        var libro = await contexto.Libros
            .FirstOrDefaultAsync(l => l.Titulo == prestamo.Libro);
        if (libro != null)
        {
            libro.CantidadDisponible++;
            contexto.Libros.Update(libro);
        }

        contexto.Prestamos.Remove(prestamo);
        return await contexto.SaveChangesAsync() > 0;
    }
}
