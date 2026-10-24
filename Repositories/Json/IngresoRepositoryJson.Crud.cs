using System.Text.Json;
using SistemaParqueos.Models.Entidades;

namespace SistemaParqueos.Repositories.Json;

public partial class IngresoRepositoryJson
{
    private static readonly SemaphoreSlim Bloqueo = new(1, 1);

    private string RutaIngresos =>
        Path.Combine(_carpeta, "ingresos.json");

    private async Task GuardarAsync(List<PRQIngresos> datos)
    {
        var contenido = datos.Select(x => new
        {
            x.Consecutivo,
            x.IdParqueo,
            x.IdAutomovil,
            x.FechaHoraEntrada,
            x.FechaHoraSalida
        });

        var json = JsonSerializer.Serialize(
            contenido,
            new JsonSerializerOptions { WriteIndented = true });

        await File.WriteAllTextAsync(RutaIngresos, json);
    }

    private async Task ValidarRelacionesAsync(PRQIngresos entidad)
    {
        var automoviles = await LeerAsync<PRQAutomoviles>(
            "automoviles.json");

        var parqueos = await LeerAsync<PRQParqueos>(
            "parqueos.json");

        if (!automoviles.Any(x => x.Id == entidad.IdAutomovil))
            throw new ArgumentException("El automovil no existe.");

        if (!parqueos.Any(x => x.Id == entidad.IdParqueo))
            throw new ArgumentException("El parqueo no existe.");

        if (entidad.FechaHoraSalida.HasValue &&
            entidad.FechaHoraSalida.Value < entidad.FechaHoraEntrada)
            throw new ArgumentException(
                "La salida no puede ser anterior a la entrada.");
    }

    public async Task<int> CrearAsync(PRQIngresos entidad)
    {
        await Bloqueo.WaitAsync();

        try
        {
            await ValidarRelacionesAsync(entidad);

            var datos = await LeerAsync<PRQIngresos>("ingresos.json");

            entidad.Consecutivo = datos.Count == 0
                ? 1
                : datos.Max(x => x.Consecutivo) + 1;

            datos.Add(entidad);
            await GuardarAsync(datos);

            return entidad.Consecutivo;
        }
        finally
        {
            Bloqueo.Release();
        }
    }

    public async Task<bool> ActualizarAsync(PRQIngresos entidad)
    {
        await Bloqueo.WaitAsync();

        try
        {
            var datos = await LeerAsync<PRQIngresos>("ingresos.json");

            var actual = datos.FirstOrDefault(
                x => x.Consecutivo == entidad.Consecutivo);

            if (actual == null)
                return false;

            await ValidarRelacionesAsync(entidad);

            actual.IdAutomovil = entidad.IdAutomovil;
            actual.IdParqueo = entidad.IdParqueo;
            actual.FechaHoraEntrada = entidad.FechaHoraEntrada;
            actual.FechaHoraSalida = entidad.FechaHoraSalida;

            await GuardarAsync(datos);
            return true;
        }
        finally
        {
            Bloqueo.Release();
        }
    }

    public async Task<bool> EliminarAsync(int id)
    {
        await Bloqueo.WaitAsync();

        try
        {
            var datos = await LeerAsync<PRQIngresos>("ingresos.json");

            var actual = datos.FirstOrDefault(
                x => x.Consecutivo == id);

            if (actual == null)
                return false;

            datos.Remove(actual);
            await GuardarAsync(datos);
            return true;
        }
        finally
        {
            Bloqueo.Release();
        }
    }
}
