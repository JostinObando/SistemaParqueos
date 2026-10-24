using Microsoft.AspNetCore.Mvc;
using SistemaParqueos.Models.Entidades;
using SistemaParqueos.Repositories.Interfaces;

namespace SistemaParqueos.Controllers;

[ApiController]
[Route("consultas")]
public class ConsultasController : ControllerBase
{
    private readonly IAutomovilRepository _automoviles;
    private readonly IParqueoRepository _parqueos;
    private readonly IIngresoRepository _ingresos;

    public ConsultasController(
        IAutomovilRepository automoviles,
        IParqueoRepository parqueos,
        IIngresoRepository ingresos)
    {
        _automoviles = automoviles;
        _parqueos = parqueos;
        _ingresos = ingresos;
    }

    [HttpGet("automoviles")]
    public async Task<IActionResult> Automoviles(
        string? color,
        int? anioInicio,
        int? anioFin,
        string? fabricante,
        string? tipo)
    {
        var resultado = await _automoviles.ConsultarAsync(
            color, anioInicio, anioFin, fabricante, tipo);

        return Ok(resultado.Select(x => new
        {
            x.Id,
            x.Color,
            x.Anio,
            x.Fabricante,
            x.Tipo
        }));
    }

    [HttpGet("automoviles/{id:int}")]
    public async Task<IActionResult> Automovil(int id)
    {
        var resultado = await _automoviles.ObtenerPorIdAsync(id);

        return resultado == null ? NotFound() : Ok(new
        {
            resultado.Id,
            resultado.Color,
            resultado.Anio,
            resultado.Fabricante,
            resultado.Tipo
        });
    }

    [HttpGet("parqueos")]
    public async Task<IActionResult> Parqueos(
        string? provincia,
        string? nombre,
        decimal? precioMinimo,
        decimal? precioMaximo)
    {
        var resultado = await _parqueos.ConsultarAsync(
            provincia, nombre, precioMinimo, precioMaximo);

        return Ok(resultado.Select(x => new
        {
            x.Id,
            x.Provincia,
            x.Nombre,
            x.PrecioPorHora
        }));
    }

    [HttpGet("parqueos/{id:int}")]
    public async Task<IActionResult> Parqueo(int id)
    {
        var resultado = await _parqueos.ObtenerPorIdAsync(id);

        return resultado == null ? NotFound() : Ok(new
        {
            resultado.Id,
            resultado.Provincia,
            resultado.Nombre,
            resultado.PrecioPorHora
        });
    }

    [HttpGet("precio/{idParqueo:int}")]
    public async Task<IActionResult> Precio(int idParqueo)
    {
        var precio = await _ingresos.ObtenerPrecioPorHoraPorParqueo(
            idParqueo);

        return precio == null ? NotFound() : Ok(new
        {
            IdParqueo = idParqueo,
            PrecioPorHora = precio
        });
    }

    [HttpGet("ingresos/tipo")]
    public async Task<IActionResult> IngresosPorTipo(
        string tipo,
        DateTime fechaInicio,
        DateTime fechaFin)
    {
        var resultado = await _ingresos.ConsultarPorTipoAsync(
            tipo, fechaInicio, fechaFin);

        return Ok(PrepararIngresos(resultado));
    }

    [HttpGet("ingresos/provincia")]
    public async Task<IActionResult> IngresosPorProvincia(
        string provincia,
        DateTime fechaInicio,
        DateTime fechaFin)
    {
        var resultado = await _ingresos.ConsultarPorProvinciaAsync(
            provincia, fechaInicio, fechaFin);

        return Ok(PrepararIngresos(resultado));
    }

    [HttpGet("ingresos/{consecutivo:int}")]
    public async Task<IActionResult> Ingreso(int consecutivo)
    {
        var ingreso = await _ingresos.ObtenerPorIdAsync(consecutivo);

        return ingreso == null
            ? NotFound()
            : Ok(PrepararIngresos(new[] { ingreso }).First());
    }

    private static IEnumerable<object> PrepararIngresos(
        IEnumerable<PRQIngresos> ingresos)
    {
        return ingresos.Select(x => new
        {
            x.Consecutivo,
            x.IdAutomovil,
            x.IdParqueo,
            TipoAutomovil = x.IdAutomovilNavigation?.Tipo,
            Provincia = x.IdParqueoNavigation?.Provincia,
            x.FechaHoraEntrada,
            x.FechaHoraSalida,
            x.DuracionMinutos,
            x.DuracionHoras,
            x.MontoTotalPagar
        });
    }
}
