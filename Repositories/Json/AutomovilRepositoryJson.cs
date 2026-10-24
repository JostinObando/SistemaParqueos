using System.Text.Json;
using SistemaParqueos.Models.Entidades;
using SistemaParqueos.Repositories.Interfaces;

namespace SistemaParqueos.Repositories.Json;

public partial class AutomovilRepositoryJson : IAutomovilRepository
{
    private readonly string _ruta;

    public AutomovilRepositoryJson(IWebHostEnvironment entorno)
    {
        _ruta = Path.Combine(
            entorno.ContentRootPath, "DatosJson", "automoviles.json");
    }

    private async Task<List<PRQAutomoviles>> LeerAsync()
    {
        var json = await File.ReadAllTextAsync(_ruta);

        return JsonSerializer.Deserialize<List<PRQAutomoviles>>(
            json) ?? new();
    }

    public async Task<PRQAutomoviles?> ObtenerPorIdAsync(int id)
    {
        var registros = await LeerAsync();
        return registros.FirstOrDefault(x => x.Id == id);
    }

    public async Task<List<PRQAutomoviles>> ConsultarAsync(
        string? color, int? anioInicio, int? anioFin,
        string? fabricante, string? tipo)
    {
        var consulta = (await LeerAsync()).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(color))
            consulta = consulta.Where(x =>
                x.Color.Contains(color, StringComparison.OrdinalIgnoreCase));

        if (anioInicio.HasValue)
            consulta = consulta.Where(x => x.Anio >= anioInicio.Value);

        if (anioFin.HasValue)
            consulta = consulta.Where(x => x.Anio <= anioFin.Value);

        if (!string.IsNullOrWhiteSpace(fabricante))
            consulta = consulta.Where(x =>
                x.Fabricante.Contains(fabricante,
                    StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(tipo))
            consulta = consulta.Where(x =>
                x.Tipo.Contains(tipo, StringComparison.OrdinalIgnoreCase));

        return consulta.OrderBy(x => x.Id).ToList();
    }
}

