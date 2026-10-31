using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SistemaParqueos.Models.Entidades;
using SistemaParqueos.Repositories.Interfaces;

namespace SistemaParqueos.Controllers;

[ApiController]
[Route("api/automoviles")]
public class AutomovilesApiController : ControllerBase
{
    private readonly IAutomovilRepository _automoviles;
    private readonly IIngresoRepository _ingresos;

    public AutomovilesApiController(
        IAutomovilRepository automoviles,
        IIngresoRepository ingresos)
    {
        _automoviles = automoviles;
        _ingresos = ingresos;
    }

    // GET: api/automoviles
    [HttpGet]
    public async Task<IActionResult> Consultar(
        string? color,
        int? anioInicio,
        int? anioFin,
        string? fabricante,
        string? tipo)
    {
        if (anioInicio > anioFin)
            return BadRequest("El intervalo de años no es válido.");

        var automoviles = await _automoviles.ConsultarAsync(
            color, anioInicio, anioFin, fabricante, tipo);

        return Ok(automoviles.Select(Proyectar));
    }

    // GET: api/automoviles/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var automovil = await _automoviles.ObtenerPorIdAsync(id);

        return automovil == null
            ? NotFound(new { mensaje = "Automóvil no encontrado." })
            : Ok(Proyectar(automovil));
    }

    // GET: api/automoviles/1/ingresos
    // Evidencia de propiedades de navegación
    [HttpGet("{id:int}/ingresos")]
    public async Task<IActionResult> ObtenerIngresos(int id)
    {
        var automovil = await _automoviles.ObtenerPorIdAsync(id);

        if (automovil == null)
            return NotFound(new { mensaje = "Automóvil no encontrado." });

        var registros = await _ingresos.ConsultarPorTipoAsync(
            "",
            new DateTime(2000, 1, 1),
            new DateTime(2100, 1, 1));

        var relacionados = registros
            .Where(x => x.IdAutomovil == id)
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
            Automovil = Proyectar(automovil),
            Ingresos = relacionados
        });
    }

    // POST: api/automoviles
    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] AutomovilEntrada datos)
    {
        var error = Validar(datos);

        if (error != null)
            return BadRequest(new { mensaje = error });

        try
        {
            var id = await _automoviles.CrearAsync(new PRQAutomoviles
            {
                Color = datos.Color.Trim(),
                Anio = datos.Anio,
                Fabricante = datos.Fabricante.Trim(),
                Tipo = NormalizarTipo(datos.Tipo)
            });

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id },
                new { id, mensaje = "Automóvil creado correctamente." });
        }
        catch (Exception ex) when (EsConflicto(ex))
        {
            return Conflict(new
            {
                mensaje = "No se pudo crear el automóvil."
            });
        }
    }

    // PUT: api/automoviles/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id,
        [FromBody] AutomovilEntrada datos)
    {
        var error = Validar(datos);

        if (error != null)
            return BadRequest(new { mensaje = error });

        try
        {
            var actualizado = await _automoviles.ActualizarAsync(
                new PRQAutomoviles
                {
                    Id = id,
                    Color = datos.Color.Trim(),
                    Anio = datos.Anio,
                    Fabricante = datos.Fabricante.Trim(),
                    Tipo = NormalizarTipo(datos.Tipo)
                });

            return actualizado
                ? NoContent()
                : NotFound(new { mensaje = "Automóvil no encontrado." });
        }
        catch (Exception ex) when (EsConflicto(ex))
        {
            return Conflict(new
            {
                mensaje = "No se pudo actualizar el automóvil."
            });
        }
    }

    // DELETE: api/automoviles/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var eliminado = await _automoviles.EliminarAsync(id);

            return eliminado
                ? NoContent()
                : NotFound(new { mensaje = "Automóvil no encontrado." });
        }
        catch (Exception ex) when (EsConflicto(ex))
        {
            return Conflict(new
            {
                mensaje = "El automóvil tiene registros relacionados."
            });
        }
    }

    private static object Proyectar(PRQAutomoviles x) => new
    {
        x.Id,
        x.Color,
        x.Anio,
        x.Fabricante,
        x.Tipo
    };

    private static string? Validar(AutomovilEntrada datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Color) ||
            string.IsNullOrWhiteSpace(datos.Fabricante) ||
            string.IsNullOrWhiteSpace(datos.Tipo))
            return "Los datos del automóvil son obligatorios.";

        if (datos.Anio < 1900 ||
            datos.Anio > DateTime.Today.Year + 1)
            return "El año no es válido.";

        if (!new[] { "Sedan", "4x4", "Moto" }
            .Contains(datos.Tipo.Trim(), StringComparer.OrdinalIgnoreCase))
            return "El tipo debe ser Sedan, 4x4 o Moto.";

        return null;
    }

    private static string NormalizarTipo(string tipo)
    {
        return tipo.Trim().ToLowerInvariant() switch
        {
            "sedan" => "Sedan",
            "moto" => "Moto",
            _ => "4x4"
        };
    }

    private static bool EsConflicto(Exception ex) =>
        ex is InvalidOperationException ||
        ex is DbUpdateException db &&
        db.InnerException is PostgresException pg &&
        pg.SqlState is "23503" or "23505" or "23514";
}
