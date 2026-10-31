using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaParqueos.Models.Entidades;
using SistemaParqueos.Repositories.Interfaces;

namespace SistemaParqueos.Controllers;

[ApiController]
[Route("api/ingresos")]
public class IngresosApiController : ControllerBase
{
    private readonly IIngresoRepository _ingresos;
    private readonly IAutomovilRepository _automoviles;
    private readonly IParqueoRepository _parqueos;

    public IngresosApiController(
        IIngresoRepository ingresos,
        IAutomovilRepository automoviles,
        IParqueoRepository parqueos)
    {
        _ingresos = ingresos;
        _automoviles = automoviles;
        _parqueos = parqueos;
    }

    // GET: api/ingresos
    [HttpGet]
    public async Task<IActionResult> Consultar()
    {
        var datos = await _ingresos.ConsultarPorProvinciaAsync(
            "",
            new DateTime(2000, 1, 1),
            new DateTime(2100, 1, 1));

        return Ok(datos.Select(Proyectar));
    }

    // GET: api/ingresos/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var ingreso = await _ingresos.ObtenerPorIdAsync(id);

        if (ingreso == null)
        {
            return NotFound(new
            {
                mensaje = "Ingreso no encontrado."
            });
        }

        return Ok(Proyectar(ingreso));
    }

    // GET: api/ingresos/por-tipo
    [HttpGet("por-tipo")]
    public async Task<IActionResult> ConsultarPorTipo(
        string tipo,
        DateTime fechaInicio,
        DateTime fechaFin)
    {
        if (string.IsNullOrWhiteSpace(tipo) ||
            fechaInicio == default ||
            fechaFin == default ||
            fechaInicio > fechaFin)
        {
            return BadRequest(new
            {
                mensaje = "Los parametros de consulta no son validos."
            });
        }

        var datos = await _ingresos.ConsultarPorTipoAsync(
            tipo, fechaInicio, fechaFin);

        return Ok(datos.Select(Proyectar));
    }

    // GET: api/ingresos/por-provincia
    [HttpGet("por-provincia")]
    public async Task<IActionResult> ConsultarPorProvincia(
        string provincia,
        DateTime fechaInicio,
        DateTime fechaFin)
    {
        if (string.IsNullOrWhiteSpace(provincia) ||
            fechaInicio == default ||
            fechaFin == default ||
            fechaInicio > fechaFin)
        {
            return BadRequest(new
            {
                mensaje = "Los parametros de consulta no son validos."
            });
        }

        var datos = await _ingresos.ConsultarPorProvinciaAsync(
            provincia, fechaInicio, fechaFin);

        return Ok(datos.Select(Proyectar));
    }

    // GET: api/ingresos/precio/1
    [HttpGet("precio/{idParqueo:int}")]
    public async Task<IActionResult> ObtenerPrecioPorHoraPorParqueo(
        int idParqueo)
    {
        var parqueo = await _parqueos.ObtenerPorIdAsync(idParqueo);

        if (parqueo == null)
        {
            return NotFound(new
            {
                mensaje = "Parqueo no encontrado."
            });
        }

        return Ok(new
        {
            idParqueo = parqueo.Id,
            parqueo.Nombre,
            parqueo.Provincia,
            parqueo.PrecioPorHora
        });
    }

    // POST: api/ingresos
    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] IngresoApiDatos datos)
    {
        var error = await Validar(datos);

        if (error != null)
            return BadRequest(new { mensaje = error });

        try
        {
            var id = await _ingresos.CrearAsync(new PRQIngresos
            {
                IdAutomovil = datos.IdAutomovil,
                IdParqueo = datos.IdParqueo,
                FechaHoraEntrada = datos.FechaHoraEntrada,
                FechaHoraSalida = datos.FechaHoraSalida
            });

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id },
                new
                {
                    id,
                    mensaje = "Ingreso registrado correctamente."
                });
        }
        catch (Exception ex) when (
            ex is DbUpdateException or InvalidOperationException)
        {
            return Conflict(new
            {
                mensaje = "No se pudo registrar el ingreso."
            });
        }
    }

    // PUT: api/ingresos/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id,
        [FromBody] IngresoApiDatos datos)
    {
        var error = await Validar(datos);

        if (error != null)
            return BadRequest(new { mensaje = error });

        try
        {
            var actualizado = await _ingresos.ActualizarAsync(
                new PRQIngresos
                {
                    Consecutivo = id,
                    IdAutomovil = datos.IdAutomovil,
                    IdParqueo = datos.IdParqueo,
                    FechaHoraEntrada = datos.FechaHoraEntrada,
                    FechaHoraSalida = datos.FechaHoraSalida
                });

            return actualizado
                ? NoContent()
                : NotFound(new
                {
                    mensaje = "Ingreso no encontrado."
                });
        }
        catch (Exception ex) when (
            ex is DbUpdateException or InvalidOperationException)
        {
            return Conflict(new
            {
                mensaje = "No se pudo actualizar el ingreso."
            });
        }
    }

    // DELETE: api/ingresos/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var eliminado = await _ingresos.EliminarAsync(id);

            return eliminado
                ? NoContent()
                : NotFound(new
                {
                    mensaje = "Ingreso no encontrado."
                });
        }
        catch (Exception ex) when (
            ex is DbUpdateException or InvalidOperationException)
        {
            return Conflict(new
            {
                mensaje = "No se pudo eliminar el ingreso."
            });
        }
    }

    private async Task<string?> Validar(IngresoApiDatos datos)
    {
        if (datos.IdAutomovil <= 0 ||
            datos.IdParqueo <= 0 ||
            datos.FechaHoraEntrada == default)
        {
            return "Los datos del ingreso son obligatorios.";
        }

        if (datos.FechaHoraSalida.HasValue &&
            datos.FechaHoraSalida.Value < datos.FechaHoraEntrada)
        {
            return "La salida no puede ser anterior a la entrada.";
        }

        if (await _automoviles.ObtenerPorIdAsync(
            datos.IdAutomovil) == null)
        {
            return "El automovil indicado no existe.";
        }

        if (await _parqueos.ObtenerPorIdAsync(
            datos.IdParqueo) == null)
        {
            return "El parqueo indicado no existe.";
        }

        return null;
    }

    private static object Proyectar(PRQIngresos ingreso) => new
    {
        ingreso.Consecutivo,
        ingreso.IdAutomovil,
        ingreso.IdParqueo,
        ingreso.FechaHoraEntrada,
        ingreso.FechaHoraSalida,

        ingreso.DuracionMinutos,
        ingreso.DuracionHoras,
        ingreso.MontoTotalPagar,

        Automovil = ingreso.IdAutomovilNavigation == null
            ? null
            : new
            {
                ingreso.IdAutomovilNavigation.Id,
                ingreso.IdAutomovilNavigation.Color,
                ingreso.IdAutomovilNavigation.Anio,
                ingreso.IdAutomovilNavigation.Fabricante,
                ingreso.IdAutomovilNavigation.Tipo
            },

        Parqueo = ingreso.IdParqueoNavigation == null
            ? null
            : new
            {
                ingreso.IdParqueoNavigation.Id,
                ingreso.IdParqueoNavigation.Nombre,
                ingreso.IdParqueoNavigation.Provincia,
                ingreso.IdParqueoNavigation.PrecioPorHora
            }
    };
}

public record IngresoApiDatos(
    int IdAutomovil,
    int IdParqueo,
    DateTime FechaHoraEntrada,
    DateTime? FechaHoraSalida);
