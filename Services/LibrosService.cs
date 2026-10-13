using Aplicada1.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RegistroLibro.Context;
using RegistroLibro.Models;
using System.Linq.Expressions;

namespace RegistroLibro.Services;

public class LibrosService : IServiceResult<Libro, int>
{
    private readonly IDbContextFactory<RegistroContext> _factory;

    public LibrosService(IDbContextFactory<RegistroContext> factory)
    {
        _factory = factory;
    }

    public async Task<Result> Guardar(Libro libro)
    {
        if (string.IsNullOrWhiteSpace(libro.Titulo) ||
            string.IsNullOrWhiteSpace(libro.Autor))
        {
            return Result.Failure(new Error("VALIDATION", "El titulo y el autor son obligatorios."));
        }

        await using var contexto = await _factory.CreateDbContextAsync();

        libro.Titulo = libro.Titulo.Trim();
        libro.Autor = libro.Autor.Trim();

        bool repetido = await contexto.Libros.AnyAsync(l =>
            l.Titulo == libro.Titulo &&
            l.LibroId != libro.LibroId);

        if (repetido)
            return Result.Failure(new Error("DUPLICATE_TITLE", "Ya existe un libro con ese titulo."));

        if (libro.LibroId == 0)
            contexto.Libros.Add(libro);
        else
        {
            var existente = await contexto.Libros.FindAsync(libro.LibroId);
            if (existente is null)
                return Result.Failure(new Error("NOT_FOUND", "No se encontro el libro que deseas actualizar."));

            existente.Titulo = libro.Titulo;
            existente.Autor = libro.Autor;
            existente.AnoPublicacion = libro.AnoPublicacion;
            existente.CantidadDisponible = libro.CantidadDisponible;
        }

        try
        {
            await contexto.SaveChangesAsync();
            return Result.Success();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is SqlException sql &&
            (sql.Number == 2601 || sql.Number == 2627))
        {
            return Result.Failure(new Error("DUPLICATE_TITLE", "Ya existe un libro con ese titulo."));
        }
        catch (DbUpdateException)
        {
            return Result.Failure(new Error("DATABASE_ERROR", "No se pudo guardar el libro."));
        }
    }

    public async Task<Result<List<Libro>>> GetList(Expression<Func<Libro, bool>> criterio)
    {
        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();
            var libros = await contexto.Libros
                .Where(criterio)
                .AsNoTracking()
                .OrderBy(l => l.Titulo)
                .ToListAsync();

            return Result.Success(libros);
        }
        catch
        {
            return Result.Failure<List<Libro>>(new Error("DATABASE_ERROR", "No se pudieron consultar los libros."));
        }
    }

    public async Task<Result<Libro?>> Buscar(int id)
    {
        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();
            var libro = await contexto.Libros
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.LibroId == id);

            return libro is null
                ? Result.Failure<Libro?>(new Error("NOT_FOUND", "No se encontro el libro."))
                : Result.Success<Libro?>(libro);
        }
        catch
        {
            return Result.Failure<Libro?>(new Error("DATABASE_ERROR", "No se pudo consultar el libro."));
        }
    }

    public async Task<Result> Eliminar(int id)
    {
        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();
            var libro = await contexto.Libros.FindAsync(id);

            if (libro is null)
                return Result.Failure(new Error("NOT_FOUND", "No se encontro el libro."));

            var tienePrestamoPendiente = await contexto.Prestamos
                .AnyAsync(p => p.Libro == libro.Titulo && p.FechaEntrega == null);

            if (tienePrestamoPendiente)
                return Result.Failure(new Error("LOAN_PENDING", "No se puede eliminar un libro con prestamos pendientes."));

            contexto.Libros.Remove(libro);
            await contexto.SaveChangesAsync();
            return Result.Success();
        }
        catch
        {
            return Result.Failure(new Error("DATABASE_ERROR", "No se pudo eliminar el libro."));
        }
    }

    public async Task<Result<List<LibroConPromedio>>> ObtenerLibrosConPromedioDiasPrestado()
    {
        try
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

            var resultado = libros.Select(libro =>
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

            return Result.Success(resultado);
        }
        catch
        {
            return Result.Failure<List<LibroConPromedio>>(
                new Error("DATABASE_ERROR", "No se pudo calcular el promedio de dias prestados."));
        }
    }
}