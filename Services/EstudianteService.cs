using Aplicada1.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RegistroLibro.Context;
using RegistroLibro.Models;
using System.Linq.Expressions;

namespace RegistroLibro.Services;
public class EstudianteService : IService<Estudiante, int>
{
    private readonly IDbContextFactory<RegistroContext> _factory;

    public EstudianteService(
        IDbContextFactory<RegistroContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<Estudiante>> Listar()
    {
        return await GetList(e => true);
    }

    public async Task<List<Estudiante>> GetList(
        Expression<Func<Estudiante, bool>> criterio)
    {
        using var contexto = await _factory.CreateDbContextAsync();

        return await contexto.Estudiantes
            .Where(criterio)
            .AsNoTracking()
            .OrderBy(e => e.Nombres)
            .ToListAsync();
    }

    public async Task<Estudiante?> Buscar(int id)
    {
        using var contexto = await _factory.CreateDbContextAsync();

        return await contexto.Estudiantes
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EstudianteId == id);
    }
    public async Task<bool> ExisteNombre(
        string nombres, int estudianteId = 0)
    {
        using var contexto = await _factory.CreateDbContextAsync();

        var nombre = nombres.Trim();

        return await contexto.Estudiantes.AnyAsync(e =>
            e.Nombres == nombre &&
            e.EstudianteId != estudianteId);
    }
    public async Task<bool> Guardar(Estudiante estudiante)
    {
        using var contexto = await _factory.CreateDbContextAsync();

        estudiante.Nombres = estudiante.Nombres.Trim();
        estudiante.Direccion = estudiante.Direccion.Trim();
        estudiante.Email = estudiante.Email.Trim();

        bool repetido = await contexto.Estudiantes.AnyAsync(e =>
            e.Nombres == estudiante.Nombres &&
            e.EstudianteId != estudiante.EstudianteId);

        if (repetido)
            return false;

        if (estudiante.EstudianteId == 0)
        {
            contexto.Estudiantes.Add(estudiante);
        }
        else
        {
            contexto.Estudiantes.Update(estudiante);
        }

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
      public async Task<bool> Eliminar(int id)
    {
        using var contexto = await _factory.CreateDbContextAsync();

        var estudiante = await contexto.Estudiantes.FindAsync(id);

        if (estudiante is null)
            return false;

        contexto.Estudiantes.Remove(estudiante);

        return await contexto.SaveChangesAsync() > 0;
    }
}