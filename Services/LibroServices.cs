using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RegistroLibro.Context;
using RegistroLibro.Models;

namespace RegistroLibro.Services;

public class LibroService
{
    private readonly IDbContextFactory<RegistroContext> _factory;

    public LibroService(IDbContextFactory<RegistroContext> factory)
    {
        _factory = factory;
    }
    public async Task<List<Libro>> Listar()
    {
        using var contexto = await _factory.CreateDbContextAsync();

        return await contexto.Libros
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
        bool repetido = await contexto.Libros.AnyAsync(l => l.Titulo == libro.Titulo && l.LibroId != libro.LibroId);

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
    public async Task Eliminar(int id)
    {
        using var contexto = await _factory.CreateDbContextAsync();

        var libro = await contexto.Libros.FindAsync(id);

        if (libro is not null)
        {
            contexto.Libros.Remove(libro);
            await contexto.SaveChangesAsync();
        }
    }
}