using System.Text.Json;
using SistemaParqueos.Models.Entidades;

namespace SistemaParqueos.Repositories.Json;

public partial class ParqueoRepositoryJson
{
    private static readonly SemaphoreSlim Bloqueo = new(1, 1);

    private async Task GuardarAsync(List<PRQParqueos> datos)
    {
        var contenido = datos.Select(x => new
        {
            x.Id,
            x.Provincia,
            x.Nombre,
            x.PrecioPorHora
        });

        var json = JsonSerializer.Serialize(
            contenido,
            new JsonSerializerOptions { WriteIndented = true });

        await File.WriteAllTextAsync(_ruta, json);
    }

    public async Task<int> CrearAsync(PRQParqueos entidad)
    {
        await Bloqueo.WaitAsync();

        try
        {
            var datos = await LeerAsync();
            entidad.Id = datos.Count == 0 ? 1 : datos.Max(x => x.Id) + 1;

            datos.Add(entidad);
            await GuardarAsync(datos);
            return entidad.Id;
        }
        finally
        {
            Bloqueo.Release();
        }
    }

    public async Task<bool> ActualizarAsync(PRQParqueos entidad)
    {
        await Bloqueo.WaitAsync();

        try
        {
            var datos = await LeerAsync();
            var actual = datos.FirstOrDefault(x => x.Id == entidad.Id);

            if (actual == null)
                return false;

            actual.Provincia = entidad.Provincia;
            actual.Nombre = entidad.Nombre;
            actual.PrecioPorHora = entidad.PrecioPorHora;

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
            var datos = await LeerAsync();
            var actual = datos.FirstOrDefault(x => x.Id == id);

            if (actual == null)
                return false;

            var carpeta = Path.GetDirectoryName(_ruta)!;
            var rutaIngresos = Path.Combine(carpeta, "ingresos.json");

            var ingresos = JsonSerializer.Deserialize<List<PRQIngresos>>(
                await File.ReadAllTextAsync(rutaIngresos)) ?? new();

            if (ingresos.Any(x => x.IdParqueo == id))
                throw new InvalidOperationException(
                    "No se puede eliminar un parqueo con ingresos registrados.");

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
