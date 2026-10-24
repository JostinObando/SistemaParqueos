using Microsoft.EntityFrameworkCore;
using SistemaParqueos.Models.Entidades;

namespace SistemaParqueos.Repositories.BaseDatos;

public partial class AutomovilRepositoryBD
{
    public async Task<int> CrearAsync(PRQAutomoviles entidad)
    {
        _context.Set<PRQAutomoviles>().Add(entidad);
        await _context.SaveChangesAsync();

        return entidad.Id;
    }

    public async Task<bool> ActualizarAsync(PRQAutomoviles entidad)
    {
        var actual = await _context.Set<PRQAutomoviles>()
            .FindAsync(entidad.Id);

        if (actual == null)
            return false;

        _context.Entry(actual).CurrentValues.SetValues(entidad);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var entidad = await _context.Set<PRQAutomoviles>()
            .FindAsync(id);

        if (entidad == null)
            return false;

        _context.Set<PRQAutomoviles>().Remove(entidad);

        await _context.SaveChangesAsync();
        return true;
    }
}
