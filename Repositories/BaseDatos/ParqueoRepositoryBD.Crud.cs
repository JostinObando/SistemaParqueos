using Microsoft.EntityFrameworkCore;
using SistemaParqueos.Models.Entidades;

namespace SistemaParqueos.Repositories.BaseDatos;

public partial class ParqueoRepositoryBD
{
    public async Task<int> CrearAsync(PRQParqueos entidad)
    {
        _context.Set<PRQParqueos>().Add(entidad);
        await _context.SaveChangesAsync();

        return entidad.Id;
    }

    public async Task<bool> ActualizarAsync(PRQParqueos entidad)
    {
        var actual = await _context.Set<PRQParqueos>()
            .FindAsync(entidad.Id);

        if (actual == null)
            return false;

        _context.Entry(actual).CurrentValues.SetValues(entidad);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var entidad = await _context.Set<PRQParqueos>()
            .FindAsync(id);

        if (entidad == null)
            return false;

        _context.Set<PRQParqueos>().Remove(entidad);

        await _context.SaveChangesAsync();
        return true;
    }
}
