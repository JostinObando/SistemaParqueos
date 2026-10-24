using Microsoft.EntityFrameworkCore;
using SistemaParqueos.Models.Entidades;

namespace SistemaParqueos.Repositories.BaseDatos;

public partial class IngresoRepositoryBD
{
    public async Task<int> CrearAsync(PRQIngresos entidad)
    {
        _context.Set<PRQIngresos>().Add(entidad);

        await _context.SaveChangesAsync();

        return entidad.Consecutivo;
    }

    public async Task<bool> ActualizarAsync(PRQIngresos entidad)
    {
        var actual = await _context.Set<PRQIngresos>()
            .FindAsync(entidad.Consecutivo);

        if (actual == null)
            return false;

        actual.IdParqueo = entidad.IdParqueo;
        actual.IdAutomovil = entidad.IdAutomovil;
        actual.FechaHoraEntrada = entidad.FechaHoraEntrada;
        actual.FechaHoraSalida = entidad.FechaHoraSalida;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var entidad = await _context.Set<PRQIngresos>()
            .FindAsync(id);

        if (entidad == null)
            return false;

        _context.Set<PRQIngresos>().Remove(entidad);

        await _context.SaveChangesAsync();

        return true;
    }
}
