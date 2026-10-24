using Microsoft.EntityFrameworkCore;
using SistemaParqueos.Data;
using SistemaParqueos.Models.Entidades;
using SistemaParqueos.Repositories.Interfaces;

namespace SistemaParqueos.Repositories.BaseDatos;

public partial class AutomovilRepositoryBD : IAutomovilRepository
{
    private readonly ParqueosContext _context;

    public AutomovilRepositoryBD(ParqueosContext context)
    {
        _context = context;
    }

    public async Task<PRQAutomoviles?> ObtenerPorIdAsync(int id)
    {
        return await _context.Set<PRQAutomoviles>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<PRQAutomoviles>> ConsultarAsync(
        string? color,
        int? anioInicio,
        int? anioFin,
        string? fabricante,
        string? tipo)
    {
        var consulta = _context.Set<PRQAutomoviles>()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(color))
            consulta = consulta.Where(x =>
                EF.Functions.ILike(x.Color, $"%{color}%"));

        if (anioInicio.HasValue)
            consulta = consulta.Where(x => x.Anio >= anioInicio.Value);

        if (anioFin.HasValue)
            consulta = consulta.Where(x => x.Anio <= anioFin.Value);

        if (!string.IsNullOrWhiteSpace(fabricante))
            consulta = consulta.Where(x =>
                EF.Functions.ILike(x.Fabricante, $"%{fabricante}%"));

        if (!string.IsNullOrWhiteSpace(tipo))
            consulta = consulta.Where(x =>
                EF.Functions.ILike(x.Tipo, $"%{tipo}%"));

        return await consulta.OrderBy(x => x.Id).ToListAsync();
    }
}

