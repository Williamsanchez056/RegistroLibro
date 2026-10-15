using Aplicada1.Core;
using Microsoft.EntityFrameworkCore;
using RegistroLibro.Context;
using RegistroLibro.Models;
using System.Linq.Expressions;

namespace RegistroLibro.Services;

public class EntradasService : IServiceResult<Entrada, int>
{
    private readonly IDbContextFactory<RegistroContext> _factory;

    public EntradasService(IDbContextFactory<RegistroContext> factory)
    {
        _factory = factory;
    }

    public async Task<Result> Guardar(Entrada entrada)
    {
        if (entrada.Detalles is null || entrada.Detalles.Count == 0)
            return Result.Failure(new Error("DETAILS_REQUIRED", "Agrega por lo menos un libro a la entrada."));

        if (entrada.Detalles.Any(detalle => detalle.LibroId <= 0 || detalle.Cantidad <= 0))
            return Result.Failure(new Error("INVALID_DETAILS", "Cada detalle debe tener un libro y una cantidad mayor que cero."));

        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();
            await using var transaccion = await contexto.Database.BeginTransactionAsync();

            Entrada? entradaActual = null;
            var cantidadesAnteriores = new Dictionary<int, int>();

            if (entrada.EntradaId > 0)
            {
                entradaActual = await contexto.Entradas
                    .Include(e => e.Detalles)
                    .FirstOrDefaultAsync(e => e.EntradaId == entrada.EntradaId);

                if (entradaActual is null)
                    return Result.Failure(new Error("NOT_FOUND", "No se encontro la entrada que deseas modificar."));

                cantidadesAnteriores = ObtenerCantidadesPorLibro(entradaActual.Detalles);
            }

            var cantidadesNuevas = ObtenerCantidadesPorLibro(entrada.Detalles);
            var librosIds = cantidadesAnteriores.Keys
                .Union(cantidadesNuevas.Keys)
                .ToList();

            var libros = await contexto.Libros
                .Where(libro => librosIds.Contains(libro.LibroId))
                .ToDictionaryAsync(libro => libro.LibroId);

            if (libros.Count != librosIds.Count)
                return Result.Failure(new Error("BOOK_NOT_FOUND", "Uno de los libros de la entrada ya no existe."));

            var cantidadesFinales = new Dictionary<int, int>();

            foreach (var libroId in librosIds)
            {
                var cambio = cantidadesNuevas.GetValueOrDefault(libroId)
                    - cantidadesAnteriores.GetValueOrDefault(libroId);
                var cantidadFinal = libros[libroId].CantidadDisponible + cambio;

                if (cantidadFinal < 0)
                {
                    return Result.Failure(new Error(
                        "INSUFFICIENT_STOCK",
                        $"No se puede guardar la entrada porque la cantidad del libro '{libros[libroId].Titulo}' quedaria negativa."));
                }

                cantidadesFinales[libroId] = cantidadFinal;
            }

            foreach (var cantidad in cantidadesFinales)
                libros[cantidad.Key].CantidadDisponible = cantidad.Value;

            var total = entrada.Detalles.Sum(detalle => detalle.Cantidad);

            if (entradaActual is null)
            {
                var nuevaEntrada = new Entrada
                {
                    Fecha = entrada.Fecha,
                    Total = total,
                    Detalles = entrada.Detalles.Select(detalle => new EntradasDetalle
                    {
                        LibroId = detalle.LibroId,
                        Cantidad = detalle.Cantidad
                    }).ToList()
                };

                contexto.Entradas.Add(nuevaEntrada);
            }
            else
            {
                entradaActual.Fecha = entrada.Fecha;
                entradaActual.Total = total;

                contexto.EntradasDetalle.RemoveRange(entradaActual.Detalles);
                entradaActual.Detalles.Clear();

                foreach (var detalle in entrada.Detalles)
                {
                    entradaActual.Detalles.Add(new EntradasDetalle
                    {
                        EntradaId = entradaActual.EntradaId,
                        LibroId = detalle.LibroId,
                        Cantidad = detalle.Cantidad
                    });
                }
            }

            await contexto.SaveChangesAsync();
            await transaccion.CommitAsync();

            return Result.Success();
        }
        catch
        {
            return Result.Failure(new Error("DATABASE_ERROR", "No se pudo guardar la entrada."));
        }
    }

    public async Task<Result<List<Entrada>>> GetList(Expression<Func<Entrada, bool>> criterio)
    {
        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();

            var entradas = await contexto.Entradas
                .Include(entrada => entrada.Detalles)
                .ThenInclude(detalle => detalle.Libro)
                .Where(criterio)
                .AsNoTracking()
                .OrderByDescending(entrada => entrada.Fecha)
                .ThenByDescending(entrada => entrada.EntradaId)
                .ToListAsync();

            return Result.Success(entradas);
        }
        catch
        {
            return Result.Failure<List<Entrada>>(
                new Error("DATABASE_ERROR", "No se pudieron consultar las entradas."));
        }
    }

    public async Task<Result<Entrada?>> Buscar(int id)
    {
        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();

            var entrada = await contexto.Entradas
                .Include(item => item.Detalles)
                .ThenInclude(detalle => detalle.Libro)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.EntradaId == id);

            return entrada is null
                ? Result.Failure<Entrada?>(new Error("NOT_FOUND", "No se encontro la entrada."))
                : Result.Success<Entrada?>(entrada);
        }
        catch
        {
            return Result.Failure<Entrada?>(
                new Error("DATABASE_ERROR", "No se pudo consultar la entrada."));
        }
    }

    public async Task<Result> Eliminar(int id)
    {
        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();
            await using var transaccion = await contexto.Database.BeginTransactionAsync();

            var entrada = await contexto.Entradas
                .Include(item => item.Detalles)
                .FirstOrDefaultAsync(item => item.EntradaId == id);

            if (entrada is null)
                return Result.Failure(new Error("NOT_FOUND", "No se encontro la entrada."));

            var cantidadesPorLibro = ObtenerCantidadesPorLibro(entrada.Detalles);
            var librosIds = cantidadesPorLibro.Keys.ToList();

            var libros = await contexto.Libros
                .Where(libro => librosIds.Contains(libro.LibroId))
                .ToDictionaryAsync(libro => libro.LibroId);

            if (libros.Count != librosIds.Count)
                return Result.Failure(new Error("BOOK_NOT_FOUND", "No se encontro uno de los libros de la entrada."));

            foreach (var cantidad in cantidadesPorLibro)
            {
                var libro = libros[cantidad.Key];
                if (libro.CantidadDisponible - cantidad.Value < 0)
                {
                    return Result.Failure(new Error(
                        "INSUFFICIENT_STOCK",
                        $"No se puede eliminar la entrada porque el inventario de '{libro.Titulo}' quedaria negativo."));
                }
            }

            foreach (var cantidad in cantidadesPorLibro)
                libros[cantidad.Key].CantidadDisponible -= cantidad.Value;

            contexto.Entradas.Remove(entrada);
            await contexto.SaveChangesAsync();
            await transaccion.CommitAsync();

            return Result.Success();
        }
        catch
        {
            return Result.Failure(new Error("DATABASE_ERROR", "No se pudo eliminar la entrada."));
        }
    }

    private static Dictionary<int, int> ObtenerCantidadesPorLibro(
        IEnumerable<EntradasDetalle> detalles)
    {
        return detalles
            .GroupBy(detalle => detalle.LibroId)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.Sum(detalle => detalle.Cantidad));
    }
}
