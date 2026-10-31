using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaParqueos.Models.Entidades;
using SistemaParqueos.Repositories.Interfaces;

namespace SistemaParqueos.Controllers;

[ApiController]
[Route("api/parqueos")]
public class ParqueosApiController : ControllerBase
{
    private readonly IParqueoRepository _parqueos;
    private readonly IIngresoRepository _ingresos;

    public ParqueosApiController(
        IParqueoRepository parqueos,
        IIngresoRepository ingresos)
    {
        _parqueos = parqueos;
        _ingresos = ingresos;
    }

    // GET: api/parqueos
    // Permite filtrar por provincia, nombre y rango de precios
    [HttpGet]
    public async Task<IActionResult> Consultar(
        string? provincia,
        string? nombre,
        decimal? precioMinimo,
        decimal? precioMaximo)
    {
        if (precioMinimo.HasValue && precioMaximo.HasValue &&
            precioMinimo > precioMaximo)
        {
            return BadRequest(new
            {
                mensaje = "El rango de precios no es valido."
            });
        }

        var resultado = await _parqueos.ConsultarAsync(
            provincia, nombre, precioMinimo, precioMaximo);

        return Ok(resultado.Select(Proyectar));
    }

    // GET: api/parqueos/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var parqueo = await _parqueos.ObtenerPorIdAsync(id);

        if (parqueo == null)
        {
            return NotFound(new
            {
                mensaje = "Parqueo no encontrado."
            });
        }

        return Ok(Proyectar(parqueo));
    }

    // GET: api/parqueos/1/ingresos
    // Muestra los ingresos relacionados y sus datos de navegacion
    [HttpGet("{id:int}/ingresos")]
    public async Task<IActionResult> ObtenerIngresos(int id)
    {
        var parqueo = await _parqueos.ObtenerPorIdAsync(id);

        if (parqueo == null)
        {
            return NotFound(new
            {
                mensaje = "Parqueo no encontrado."
            });
        }

        var registros = await _ingresos.ConsultarPorProvinciaAsync(
            "",
            new DateTime(2000, 1, 1),
            new DateTime(2100, 1, 1));

        var relacionados = registros
            .Where(x => x.IdParqueo == id)
            .Select(x => new
            {
                x.Consecutivo,
                x.IdAutomovil,
                x.IdParqueo,
                x.FechaHoraEntrada,
                x.FechaHoraSalida,
                x.DuracionMinutos,
                x.DuracionHoras,
                x.MontoTotalPagar,

                Automovil = x.IdAutomovilNavigation == null
                    ? null
                    : new
                    {
                        x.IdAutomovilNavigation.Id,
                        x.IdAutomovilNavigation.Color,
                        x.IdAutomovilNavigation.Anio,
                        x.IdAutomovilNavigation.Fabricante,
                        x.IdAutomovilNavigation.Tipo
                    },

                Parqueo = x.IdParqueoNavigation == null
                    ? null
                    : new
                    {
                        x.IdParqueoNavigation.Id,
                        x.IdParqueoNavigation.Nombre,
                        x.IdParqueoNavigation.Provincia,
                        x.IdParqueoNavigation.PrecioPorHora
                    }
            })
            .ToList();

        return Ok(new
        {
            Parqueo = Proyectar(parqueo),
            Ingresos = relacionados
        });
    }

    // POST: api/parqueos
    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] ParqueoApiDatos datos)
    {
        var error = Validar(datos);

        if (error != null)
            return BadRequest(new { mensaje = error });

        try
        {
            var id = await _parqueos.CrearAsync(new PRQParqueos
            {
                Provincia = datos.Provincia.Trim(),
                Nombre = datos.Nombre.Trim(),
                PrecioPorHora = datos.PrecioPorHora
            });

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id },
                new
                {
                    id,
                    mensaje = "Parqueo creado correctamente."
                });
        }
        catch (Exception ex) when (
            ex is DbUpdateException or InvalidOperationException)
        {
            return Conflict(new
            {
                mensaje = "No se pudo crear el parqueo."
            });
        }
    }

    // PUT: api/parqueos/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id,
        [FromBody] ParqueoApiDatos datos)
    {
        var error = Validar(datos);

        if (error != null)
            return BadRequest(new { mensaje = error });

        try
        {
            var actualizado = await _parqueos.ActualizarAsync(
                new PRQParqueos
                {
                    Id = id,
                    Provincia = datos.Provincia.Trim(),
                    Nombre = datos.Nombre.Trim(),
                    PrecioPorHora = datos.PrecioPorHora
                });

            return actualizado
                ? NoContent()
                : NotFound(new
                {
                    mensaje = "Parqueo no encontrado."
                });
        }
        catch (Exception ex) when (
            ex is DbUpdateException or InvalidOperationException)
        {
            return Conflict(new
            {
                mensaje = "No se pudo actualizar el parqueo."
            });
        }
    }

    // DELETE: api/parqueos/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var eliminado = await _parqueos.EliminarAsync(id);

            return eliminado
                ? NoContent()
                : NotFound(new
                {
                    mensaje = "Parqueo no encontrado."
                });
        }
        catch (Exception ex) when (
            ex is DbUpdateException or InvalidOperationException)
        {
            return Conflict(new
            {
                mensaje = "No se puede eliminar un parqueo con ingresos relacionados."
            });
        }
    }

    private static object Proyectar(PRQParqueos parqueo) => new
    {
        parqueo.Id,
        parqueo.Provincia,
        parqueo.Nombre,
        parqueo.PrecioPorHora
    };

    private static string? Validar(ParqueoApiDatos datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Provincia) ||
            string.IsNullOrWhiteSpace(datos.Nombre))
        {
            return "La provincia y el nombre son obligatorios.";
        }

        if (datos.PrecioPorHora < 0)
        {
            return "El precio por hora no puede ser negativo.";
        }

        return null;
    }
}

public record ParqueoApiDatos(
    string Provincia,
    string Nombre,
    decimal PrecioPorHora);
