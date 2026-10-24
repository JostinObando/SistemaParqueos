using System.Text.Json;
using SistemaParqueos.Models.Entidades;
using SistemaParqueos.Repositories.Interfaces;

namespace SistemaParqueos.Repositories.Json;

public partial class IngresoRepositoryJson : IIngresoRepository
{
    private readonly string _carpeta;

    public IngresoRepositoryJson(IWebHostEnvironment entorno)
    {
        _carpeta = Path.Combine(entorno.ContentRootPath, "DatosJson");
    }

    private async Task<List<T>> LeerAsync<T>(string archivo)
    {
        var ruta = Path.Combine(_carpeta, archivo);
        var json = await File.ReadAllTextAsync(ruta);

        return JsonSerializer.Deserialize<List<T>>(json) ?? new();
    }

    private async Task<List<PRQIngresos>> ObtenerIngresosAsync()
    {
        var ingresos = await LeerAsync<PRQIngresos>("ingresos.json");
        var autos = await LeerAsync<PRQAutomoviles>("automoviles.json");
        var parqueos = await LeerAsync<PRQParqueos>("parqueos.json");

        foreach (var ingreso in ingresos)
        {
            ingreso.IdAutomovilNavigation = autos.FirstOrDefault(
                x => x.Id == ingreso.IdAutomovil)!;

            ingreso.IdParqueoNavigation = parqueos.FirstOrDefault(
                x => x.Id == ingreso.IdParqueo)!;
        }

        return ingresos;
    }

    public async Task<PRQIngresos?> ObtenerPorIdAsync(int consecutivo)
    {
        var ingresos = await ObtenerIngresosAsync();

        return ingresos.FirstOrDefault(
            x => x.Consecutivo == consecutivo);
    }

    public async Task<decimal?> ObtenerPrecioPorHoraPorParqueo(int idParqueo)
    {
        var parqueos = await LeerAsync<PRQParqueos>("parqueos.json");

        return parqueos
            .Where(x => x.Id == idParqueo)
            .Select(x => (decimal?)x.PrecioPorHora)
            .FirstOrDefault();
    }

    public async Task<List<PRQIngresos>> ConsultarPorTipoAsync(
        string tipo, DateTime fechaInicio, DateTime fechaFin)
    {
        var ingresos = await ObtenerIngresosAsync();

        var resultado = ingresos
            .Where(x =>
                x.IdAutomovilNavigation != null &&
                x.IdAutomovilNavigation.Tipo.Contains(
                    tipo, StringComparison.OrdinalIgnoreCase) &&
                x.FechaHoraEntrada >= fechaInicio &&
                x.FechaHoraEntrada <= fechaFin)
            .OrderBy(x => x.FechaHoraEntrada)
            .ToList();

        await CompletarPreciosAsync(resultado);

        return resultado;
    }

    public async Task<List<PRQIngresos>> ConsultarPorProvinciaAsync(
        string provincia, DateTime fechaInicio, DateTime fechaFin)
    {
        var ingresos = await ObtenerIngresosAsync();

        var resultado = ingresos
            .Where(x =>
                x.IdParqueoNavigation != null &&
                x.IdParqueoNavigation.Provincia.Contains(
                    provincia, StringComparison.OrdinalIgnoreCase) &&
                x.FechaHoraEntrada >= fechaInicio &&
                x.FechaHoraEntrada <= fechaFin)
            .OrderBy(x => x.FechaHoraEntrada)
            .ToList();

        await CompletarPreciosAsync(resultado);

        return resultado;
    }

    private async Task CompletarPreciosAsync(List<PRQIngresos> ingresos)
    {
        foreach (var grupo in ingresos.GroupBy(x => x.IdParqueo))
        {
            var precio = await ObtenerPrecioPorHoraPorParqueo(grupo.Key);

            if (!precio.HasValue)
                continue;

            foreach (var ingreso in grupo)
                ingreso.IdParqueoNavigation.PrecioPorHora = precio.Value;
        }
    }
}

