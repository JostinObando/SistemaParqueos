using SistemaParqueos.Models.Entidades;

namespace SistemaParqueos.Repositories.Interfaces;

public interface IParqueoRepository
{
    Task<PRQParqueos?> ObtenerPorIdAsync(int id);

    Task<List<PRQParqueos>> ConsultarAsync(
        string? provincia,
        string? nombre,
        decimal? precioMinimo,
        decimal? precioMaximo
    );

    Task<int> CrearAsync(PRQParqueos entidad);

    Task<bool> ActualizarAsync(PRQParqueos entidad);

    Task<bool> EliminarAsync(int id);
}

