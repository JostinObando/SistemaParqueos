using System.Text.Json;
using SistemaParqueos.Models.Entidades;
using SistemaParqueos.Repositories.Interfaces;

namespace SistemaParqueos.Repositories.Json;

public partial class ParqueoRepositoryJson : IParqueoRepository
{
    private readonly string _ruta;

    public ParqueoRepositoryJson(IWebHostEnvironment entorno)
    {
        _ruta = Path.Combine(
            entorno.ContentRootPath, "DatosJson", "parqueos.json");
    }

    private async Task<List<PRQParqueos>> LeerAsync()
    {
        var json = await File.ReadAllTextAsync(_ruta);

        return JsonSerializer.Deserialize<List<PRQParqueos>>(
            json) ?? new();
    }

    public async Task<PRQParqueos?> ObtenerPorIdAsync(int id)
    {
        var registros = await LeerAsync();
        return registros.FirstOrDefault(x => x.Id == id);
    }

    public async Task<List<PRQParqueos>> ConsultarAsync(
        string? provincia, string? nombre,
        decimal? precioMinimo, decimal? precioMaximo)
    {
        var consulta = (await LeerAsync()).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(provincia))
            consulta = consulta.Where(x =>
                x.Provincia.Contains(provincia,
                    StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(nombre))
            consulta = consulta.Where(x =>
                x.Nombre.Contains(nombre,
                    StringComparison.OrdinalIgnoreCase));

        if (precioMinimo.HasValue)
            consulta = consulta.Where(x =>
                x.PrecioPorHora >= precioMinimo.Value);

        if (precioMaximo.HasValue)
            consulta = consulta.Where(x =>
                x.PrecioPorHora <= precioMaximo.Value);

        return consulta.OrderBy(x => x.Id).ToList();
    }
}

