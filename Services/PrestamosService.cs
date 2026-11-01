using Aplicada1.Core;
using Microsoft.EntityFrameworkCore;
using RegistroLibro.Context;
using RegistroLibro.Models;
using System.Linq.Expressions;

namespace RegistroLibro.Services;

public class PrestamosService : IServiceResult<Prestamo, int>
{
    private readonly IDbContextFactory<RegistroContext> _factory;

    public PrestamosService(IDbContextFactory<RegistroContext> factory)
    {
        _factory = factory;
    }

    public async Task<Result> Guardar(Prestamo prestamo)
    {
        if (string.IsNullOrWhiteSpace(prestamo.Libro) ||
            string.IsNullOrWhiteSpace(prestamo.Estudiante))
        {
            return Result.Failure(new Error("VALIDATION", "Debes seleccionar un libro y un estudiante."));
        }

        if (prestamo.FechaDevolucion.Date < prestamo.FechaPrestamo.Date)
            return Result.Failure(new Error("INVALID_DATE", "La fecha limite no puede ser anterior a la fecha del prestamo."));

        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();

            var titulo = prestamo.Libro.Trim();
            var nombreEstudiante = prestamo.Estudiante.Trim();

            var libro = await contexto.Libros
                .FirstOrDefaultAsync(l => l.Titulo == titulo);

            if (libro is null)
                return Result.Failure(new Error("BOOK_NOT_FOUND", "El libro seleccionado no existe."));

            var estudiante = await contexto.Estudiantes
                .FirstOrDefaultAsync(e => e.Nombres == nombreEstudiante);

            if (estudiante is null)
                return Result.Failure(new Error("STUDENT_NOT_FOUND", "El estudiante seleccionado no existe."));

            Prestamo? existente = null;
            Libro? libroAnterior = null;

            if (prestamo.PrestamoId > 0)
            {
                existente = await contexto.Prestamos
                    .FirstOrDefaultAsync(p => p.PrestamoId == prestamo.PrestamoId);

                if (existente is null)
                    return Result.Failure(new Error("NOT_FOUND", "No se encontro el prestamo."));

                if (existente.FechaEntrega.HasValue)
                    return Result.Failure(new Error("RETURNED_LOAN", "No se puede modificar un prestamo que ya fue devuelto."));

                libroAnterior = await contexto.Libros
                    .FirstOrDefaultAsync(l => l.Titulo == existente.Libro);

                if (libroAnterior is null)
                    return Result.Failure(new Error("BOOK_NOT_FOUND", "No se encontro el libro asociado al prestamo actual."));
            }

            bool otroPrestamoActivo = await contexto.Prestamos.AnyAsync(p =>
                p.Estudiante == estudiante.Nombres &&
                p.FechaEntrega == null &&
                p.PrestamoId != prestamo.PrestamoId);

            if (otroPrestamoActivo)
                return Result.Failure(new Error("ACTIVE_LOAN_EXISTS", "Este estudiante ya tiene un prestamo pendiente."));

            bool cambiaLibro = libroAnterior is not null && libroAnterior.LibroId != libro.LibroId;

            if (libroAnterior is null || cambiaLibro)
            {
                if (libro.CantidadDisponible <= 0)
                    return Result.Failure(new Error("OUT_OF_STOCK", "No hay ejemplares disponibles de este libro."));

                libro.CantidadDisponible--;

                if (cambiaLibro)
                    libroAnterior!.CantidadDisponible++;
            }

            if (existente is null)
            {
                contexto.Prestamos.Add(new Prestamo
                {
                    Libro = libro.Titulo,
                    Estudiante = estudiante.Nombres,
                    FechaPrestamo = prestamo.FechaPrestamo,
                    FechaDevolucion = prestamo.FechaDevolucion,
                    Observaciones = prestamo.Observaciones
                });
            }
            else
            {
                existente.Libro = libro.Titulo;
                existente.Estudiante = estudiante.Nombres;
                existente.FechaPrestamo = prestamo.FechaPrestamo;
                existente.FechaDevolucion = prestamo.FechaDevolucion;
                existente.Observaciones = prestamo.Observaciones;
            }

            await contexto.SaveChangesAsync();
            return Result.Success();
        }
        catch
        {
            return Result.Failure(new Error("DATABASE_ERROR", "No se pudo guardar el prestamo."));
        }
    }

    public async Task<Result<List<Prestamo>>> GetList(Expression<Func<Prestamo, bool>> criterio)
    {
        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();
            var prestamos = await contexto.Prestamos
                .Where(criterio)
                .AsNoTracking()
                .OrderByDescending(p => p.FechaPrestamo)
                .ToListAsync();

            return Result.Success(prestamos);
        }
        catch
        {
            return Result.Failure<List<Prestamo>>(new Error("DATABASE_ERROR", "No se pudieron consultar los prestamos."));
        }
    }

    public async Task<Result<Prestamo?>> Buscar(int id)
    {
        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();
            var prestamo = await contexto.Prestamos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PrestamoId == id);

            return prestamo is null
                ? Result.Failure<Prestamo?>(new Error("NOT_FOUND", "No se encontro el prestamo."))
                : Result.Success<Prestamo?>(prestamo);
        }
        catch
        {
            return Result.Failure<Prestamo?>(new Error("DATABASE_ERROR", "No se pudo consultar el prestamo."));
        }
    }

    public async Task<Result> Eliminar(int id)
    {
        try
        {
            await using var contexto = await _factory.CreateDbContextAsync();
            var prestamo = await contexto.Prestamos.FindAsync(id);

            if (prestamo is null)
                return Result.Failure(new Error("NOT_FOUND", "No se encontro el prestamo."));

            var tieneDevolucion = await contexto.Devoluciones.AnyAsync(d => d.PrestamoId == id);
            if (tieneDevolucion || prestamo.FechaEntrega.HasValue)
                return Result.Failure(new Error("RETURN_EXISTS", "No se puede eliminar un prestamo que ya fue devuelto."));

            var libro = await contexto.Libros
                .FirstOrDefaultAsync(l => l.Titulo == prestamo.Libro);

            if (libro is null)
                return Result.Failure(new Error("BOOK_NOT_FOUND", "No se encontro el libro asociado al prestamo."));

            libro.CantidadDisponible++;
            contexto.Prestamos.Remove(prestamo);

            await contexto.SaveChangesAsync();
            return Result.Success();
        }
        catch
        {
            return Result.Failure(new Error("DATABASE_ERROR", "No se pudo eliminar el prestamo."));
        }
    }

    private async Task<bool> Existe(int prestamoId)
    {
        await using var contexto = await _factory.CreateDbContextAsync();
        return await contexto.Prestamos.AnyAsync(prestamo =>
            prestamo.PrestamoId == prestamoId);
    }

    private async Task<bool> Insertar(Prestamo prestamo)
    {
        await using var contexto = await _factory.CreateDbContextAsync();
        contexto.Prestamos.Add(prestamo);
        return await contexto.SaveChangesAsync() > 0;
    }

    private async Task<bool> Modificar(Prestamo prestamo)
    {
        await using var contexto = await _factory.CreateDbContextAsync();
        var existente = await contexto.Prestamos.FindAsync(prestamo.PrestamoId);
        if (existente is null)
            return false;

        existente.Libro = prestamo.Libro;
        existente.Estudiante = prestamo.Estudiante;
        existente.FechaPrestamo = prestamo.FechaPrestamo;
        existente.FechaDevolucion = prestamo.FechaDevolucion;
        existente.Observaciones = prestamo.Observaciones;
        return await contexto.SaveChangesAsync() > 0;
    }

}
