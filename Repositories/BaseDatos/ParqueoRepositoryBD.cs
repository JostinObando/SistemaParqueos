using Microsoft.EntityFrameworkCore;
using SistemaParqueos.Data;
using SistemaParqueos.Models.Entidades;
using SistemaParqueos.Repositories.Interfaces;

namespace SistemaParqueos.Repositories.BaseDatos;

public class ParqueoRepositoryBD : IParqueoRepository
{
    private readonly ParqueosContext _context;

    public ParqueoRepositoryBD(ParqueosContext context)
    {
        _context = context;
    }

    public async Task<PRQParqueos?> ObtenerPorIdAsync(int id)
    {
        return await _context.Set<PRQParqueos>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<PRQParqueos>> ConsultarAsync(
        string? provincia,
        string? nombre,
        decimal? precioMinimo,
        decimal? precioMaximo)
    {
        var consulta = _context.Set<PRQParqueos>()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(provincia))
            consulta = consulta.Where(x =>
                EF.Functions.ILike(x.Provincia, $"%{provincia}%"));

        if (!string.IsNullOrWhiteSpace(nombre))
            consulta = consulta.Where(x =>
                EF.Functions.ILike(x.Nombre, $"%{nombre}%"));

        if (precioMinimo.HasValue)
            consulta = consulta.Where(x =>
                x.PrecioPorHora >= precioMinimo.Value);

        if (precioMaximo.HasValue)
            consulta = consulta.Where(x =>
                x.PrecioPorHora <= precioMaximo.Value);

        return await consulta.OrderBy(x => x.Id).ToListAsync();
    }
}
