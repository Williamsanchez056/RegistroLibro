using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RegistroLibro.Context;
using RegistroLibro.Models;
using Aplicada1.Core;
using System.Linq.Expressions;
namespace RegistroLibro.Services;

public class LibroService : IService<Libro, int>
{
    private readonly IDbContextFactory<RegistroContext> _factory;

    public LibroService(IDbContextFactory<RegistroContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<Libro>> Listar()
    {
        return await GetList(l => true);
    }

    public async Task<List<Libro>> GetList(
        Expression<Func<Libro, bool>> criterio)
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
        {
            contexto.Libros.Add(libro);
        }
        else
        {
            contexto.Libros.Update(libro);
        }

        try
        {
            await contexto.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is SqliteException sqlite &&
            sqlite.SqliteExtendedErrorCode == 2067)
        {
            return false;
        }
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
}