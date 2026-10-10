using Aplicada1.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RegistroLibro.Context;
using RegistroLibro.Models;
using System.Linq.Expressions;
using System.Linq;
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
        using var contexto = await _factory.CreateDbContextAsync();
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
        using var contexto = await _factory.CreateDbContextAsync();
        return await contexto.Libros
            .Where(criterio)
            .AsNoTracking()
            .OrderBy(l => l.Titulo)
            .ToListAsync();
    }
    public async Task<Libro?> Buscar(int id)
    {
        using var contexto = await _factory.CreateDbContextAsync();
        return await contexto.Libros
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.LibroId == id);
    }
    public async Task<bool> Eliminar(int id)
    {
        using var contexto = await _factory.CreateDbContextAsync();
        var libro = await contexto.Libros.FindAsync(id);
        if (libro is null)
            return false;
        contexto.Libros.Remove(libro);
        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<List<LibroConPromedio>> ObtenerLibrosConPromedioDiasPrestado()
    {
        using var contexto = await _factory.CreateDbContextAsync();
        var prestamos = await contexto.Prestamos
            .AsNoTracking()
            .ToListAsync();

        var resultado = prestamos
            .Where(p => !string.IsNullOrWhiteSpace(p.Libro))
            .GroupBy(p => p.Libro!)
            .Select(g => new LibroConPromedio
            {
                LibroId = 0,
                Titulo = g.Key,
                Autor = string.Empty,
                PromedioDias = g.Average(p => ((p.FechaEntrega ?? p.FechaDevolucion) - p.FechaPrestamo).TotalDays)
            })
            .OrderBy(lp => lp.Titulo)
            .ToList();

        return resultado;
    }
}

