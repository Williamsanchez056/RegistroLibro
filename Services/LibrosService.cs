using Aplicada1.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RegistroLibro.Context;
using RegistroLibro.Models;
using System.Linq.Expressions;

namespace RegistroLibro.Services;

public class LibrosService : IService<Libro, int>
{
    private readonly IDbContextFactory<RegistroContext> _factory;

    public LibrosService(IDbContextFactory<RegistroContext> factory)
    {
        _factory = factory;
    }

    public async Task<bool> Guardar(Libro libro)
    {
        await using var contexto = await _factory.CreateDbContextAsync();

        libro.Titulo = libro.Titulo.Trim();
        libro.Autor = libro.Autor.Trim();

        bool repetido = await contexto.Libros.AnyAsync(l =>
            l.Titulo == libro.Titulo &&
            l.LibroId != libro.LibroId);

        if (repetido)
            return false;

        if (libro.LibroId == 0)
            contexto.Libros.Add(libro);
        else
            contexto.Libros.Update(libro);

        try
        {
            await contexto.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is SqlException sql &&
            (sql.Number == 2601 || sql.Number == 2627))
        {
            return false;
        }
    }

    public async Task<List<Libro>> Listar()
    {
        return await GetList(l => true);
    }

    public async Task<List<Libro>> GetList(Expression<Func<Libro, bool>> criterio)
    {
        await using var contexto = await _factory.CreateDbContextAsync();

        return await contexto.Libros
            .Where(criterio)
            .AsNoTracking()
            .OrderBy(l => l.Titulo)
            .ToListAsync();
    }

    public async Task<Libro?> Buscar(int id)
    {
        await using var contexto = await _factory.CreateDbContextAsync();

        return await contexto.Libros
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.LibroId == id);
    }

    public async Task<bool> Eliminar(int id)
    {
        await using var contexto = await _factory.CreateDbContextAsync();

        var libro = await contexto.Libros.FindAsync(id);
        if (libro is null)
            return false;

        var tienePrestamoPendiente = await contexto.Prestamos
            .AnyAsync(p => p.Libro == libro.Titulo && p.FechaEntrega == null);

        if (tienePrestamoPendiente)
            return false;

        contexto.Libros.Remove(libro);
        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<List<LibroConPromedio>> ObtenerLibrosConPromedioDiasPrestado()
    {
        await using var contexto = await _factory.CreateDbContextAsync();

        var libros = await contexto.Libros
            .AsNoTracking()
            .OrderBy(l => l.Titulo)
            .ToListAsync();

        var prestamosDevueltos = await contexto.Prestamos
            .AsNoTracking()
            .Where(p => p.FechaEntrega.HasValue)
            .ToListAsync();

        var promediosPorTitulo = prestamosDevueltos
            .Where(p => !string.IsNullOrWhiteSpace(p.Libro))
            .Select(p => new
            {
                Titulo = p.Libro.Trim(),
                p.FechaPrestamo,
                FechaEntrega = p.FechaEntrega.GetValueOrDefault()
            })
            .Where(p => p.FechaEntrega.Date >= p.FechaPrestamo.Date)
            .GroupBy(p => p.Titulo, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.Average(p =>
                    (p.FechaEntrega.Date - p.FechaPrestamo.Date).TotalDays),
                StringComparer.OrdinalIgnoreCase);

        return libros.Select(libro =>
        {
            promediosPorTitulo.TryGetValue(libro.Titulo.Trim(), out var promedio);

            return new LibroConPromedio
            {
                LibroId = libro.LibroId,
                Titulo = libro.Titulo,
                Autor = libro.Autor,
                PromedioDias = promedio
            };
        }).ToList();
    }
}
