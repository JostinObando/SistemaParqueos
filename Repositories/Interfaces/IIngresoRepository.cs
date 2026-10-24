using SistemaParqueos.Models.Entidades;

namespace SistemaParqueos.Repositories.Interfaces;

public interface IIngresoRepository
{
    Task<PRQIngresos?> ObtenerPorIdAsync(int consecutivo);

    Task<decimal?> ObtenerPrecioPorHoraPorParqueo(int idParqueo);

    Task<List<PRQIngresos>> ConsultarPorTipoAsync(
        string tipo,
        DateTime fechaInicio,
        DateTime fechaFin
    );

    Task<List<PRQIngresos>> ConsultarPorProvinciaAsync(
        string provincia,
        DateTime fechaInicio,
        DateTime fechaFin
    );

    Task<int> CrearAsync(PRQIngresos entidad);

    Task<bool> ActualizarAsync(PRQIngresos entidad);

    Task<bool> EliminarAsync(int id);
}

