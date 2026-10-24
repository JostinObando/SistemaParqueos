using Microsoft.EntityFrameworkCore;
using SistemaParqueos.Data;
using SistemaParqueos.Models.Entidades;
using SistemaParqueos.Repositories.Interfaces;

namespace SistemaParqueos.Repositories.BaseDatos;

public class IngresoRepositoryBD : IIngresoRepository
{
    private readonly ParqueosContext _context;

    public IngresoRepositoryBD(ParqueosContext context)
    {
        _context = context;
    }

    public async Task<PRQIngresos?> ObtenerPorIdAsync(int consecutivo)
    {
        return await ConsultaBase()
            .FirstOrDefaultAsync(x => x.Consecutivo == consecutivo);
    }

    public async Task<decimal?> ObtenerPrecioPorHoraPorParqueo(int idParqueo)
    {
        return await _context.Set<PRQParqueos>()
            .Where(x => x.Id == idParqueo)
            .Select(x => (decimal?)x.PrecioPorHora)
            .FirstOrDefaultAsync();
    }

    public async Task<List<PRQIngresos>> ConsultarPorTipoAsync(
        string tipo,
        DateTime fechaInicio,
        DateTime fechaFin)
    {
        var registros = await ConsultaBase()
            .Where(x =>
                EF.Functions.ILike(
                    x.IdAutomovilNavigation.Tipo,
                    $"%{tipo}%") &&
                x.FechaHoraEntrada >= fechaInicio &&
                x.FechaHoraEntrada <= fechaFin)
            .OrderBy(x => x.FechaHoraEntrada)
            .ToListAsync();

        await CompletarPreciosAsync(registros);

        return registros;
    }

    public async Task<List<PRQIngresos>> ConsultarPorProvinciaAsync(
        string provincia,
        DateTime fechaInicio,
        DateTime fechaFin)
    {
        var registros = await ConsultaBase()
            .Where(x =>
                EF.Functions.ILike(
                    x.IdParqueoNavigation.Provincia,
                    $"%{provincia}%") &&
                x.FechaHoraEntrada >= fechaInicio &&
                x.FechaHoraEntrada <= fechaFin)
            .OrderBy(x => x.FechaHoraEntrada)
            .ToListAsync();

        await CompletarPreciosAsync(registros);

        return registros;
    }

    private IQueryable<PRQIngresos> ConsultaBase()
    {
        return _context.Set<PRQIngresos>()
            .AsNoTracking()
            .Include(x => x.IdAutomovilNavigation)
            .Include(x => x.IdParqueoNavigation);
    }

    private async Task CompletarPreciosAsync(List<PRQIngresos> registros)
    {
        foreach (var grupo in registros.GroupBy(x => x.IdParqueo))
        {
            var precio = await ObtenerPrecioPorHoraPorParqueo(grupo.Key);

            if (!precio.HasValue)
                continue;

            foreach (var ingreso in grupo)
                ingreso.IdParqueoNavigation.PrecioPorHora = precio.Value;
        }
    }
}
