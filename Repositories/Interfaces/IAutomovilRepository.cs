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

    Task<int> CrearAsync(PRQAutomoviles entidad);

    Task<bool> ActualizarAsync(PRQAutomoviles entidad);

    Task<bool> EliminarAsync(int id);
}

