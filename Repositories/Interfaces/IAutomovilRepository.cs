using SistemaParqueos.Models.Entidades;

namespace SistemaParqueos.Repositories.Interfaces;

public interface IAutomovilRepository
{
    Task<PRQAutomoviles?> ObtenerPorIdAsync(int id);

    Task<List<PRQAutomoviles>> ConsultarAsync(
        string? color,
        int? anioInicio,
        int? anioFin,
        string? fabricante,
        string? tipo
    );
}
