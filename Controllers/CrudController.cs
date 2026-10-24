using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SistemaParqueos.Models.Entidades;
using SistemaParqueos.Repositories.Interfaces;

namespace SistemaParqueos.Controllers;

[ApiController]
[Route("api/crud")]
public class CrudController : ControllerBase
{
    private readonly IAutomovilRepository _autos;
    private readonly IParqueoRepository _parqueos;
    private readonly IIngresoRepository _ingresos;

    public CrudController(
        IAutomovilRepository autos,
        IParqueoRepository parqueos,
        IIngresoRepository ingresos)
    {
        _autos = autos;
        _parqueos = parqueos;
        _ingresos = ingresos;
    }

    [HttpPost("automoviles")]
    public Task<IActionResult> CrearAutomovil(AutomovilEntrada datos) =>
        Ejecutar(async () =>
        {
            ValidarAutomovil(datos);
            var id = await _autos.CrearAsync(new PRQAutomoviles
            {
                Color = datos.Color.Trim(),
                Anio = datos.Anio,
                Fabricante = datos.Fabricante.Trim(),
                Tipo = datos.Tipo.Trim()
            });

            return Created($"/consultas/automoviles/{id}", new { Id = id });
        });

    [HttpPut("automoviles/{id:int}")]
    public Task<IActionResult> ActualizarAutomovil(
        int id, AutomovilEntrada datos) =>
        Ejecutar(async () =>
        {
            ValidarAutomovil(datos);
            var actualizado = await _autos.ActualizarAsync(new PRQAutomoviles
            {
                Id = id,
                Color = datos.Color.Trim(),
                Anio = datos.Anio,
                Fabricante = datos.Fabricante.Trim(),
                Tipo = datos.Tipo.Trim()
            });

            return actualizado ? NoContent() : NotFound();
        });

    [HttpDelete("automoviles/{id:int}")]
    public Task<IActionResult> EliminarAutomovil(int id) =>
        Ejecutar(async () =>
            await _autos.EliminarAsync(id) ? NoContent() : NotFound());

    [HttpPost("parqueos")]
    public Task<IActionResult> CrearParqueo(ParqueoEntrada datos) =>
        Ejecutar(async () =>
        {
            ValidarParqueo(datos);
            var id = await _parqueos.CrearAsync(new PRQParqueos
            {
                Provincia = datos.Provincia.Trim(),
                Nombre = datos.Nombre.Trim(),
                PrecioPorHora = datos.PrecioPorHora
            });

            return Created($"/consultas/parqueos/{id}", new { Id = id });
        });

    [HttpPut("parqueos/{id:int}")]
    public Task<IActionResult> ActualizarParqueo(
        int id, ParqueoEntrada datos) =>
        Ejecutar(async () =>
        {
            ValidarParqueo(datos);
            var actualizado = await _parqueos.ActualizarAsync(new PRQParqueos
            {
                Id = id,
                Provincia = datos.Provincia.Trim(),
                Nombre = datos.Nombre.Trim(),
                PrecioPorHora = datos.PrecioPorHora
            });

            return actualizado ? NoContent() : NotFound();
        });

    [HttpDelete("parqueos/{id:int}")]
    public Task<IActionResult> EliminarParqueo(int id) =>
        Ejecutar(async () =>
            await _parqueos.EliminarAsync(id) ? NoContent() : NotFound());

    [HttpPost("ingresos")]
    public Task<IActionResult> CrearIngreso(IngresoEntrada datos) =>
        Ejecutar(async () =>
        {
            await ValidarIngresoAsync(datos);
            var id = await _ingresos.CrearAsync(new PRQIngresos
            {
                IdAutomovil = datos.IdAutomovil,
                IdParqueo = datos.IdParqueo,
                FechaHoraEntrada = datos.FechaHoraEntrada,
                FechaHoraSalida = datos.FechaHoraSalida
            });

            return Created($"/consultas/ingresos/{id}",
                new { Consecutivo = id });
        });

    [HttpPut("ingresos/{id:int}")]
    public Task<IActionResult> ActualizarIngreso(
        int id, IngresoEntrada datos) =>
        Ejecutar(async () =>
        {
            await ValidarIngresoAsync(datos);
            var actualizado = await _ingresos.ActualizarAsync(new PRQIngresos
            {
                Consecutivo = id,
                IdAutomovil = datos.IdAutomovil,
                IdParqueo = datos.IdParqueo,
                FechaHoraEntrada = datos.FechaHoraEntrada,
                FechaHoraSalida = datos.FechaHoraSalida
            });

            return actualizado ? NoContent() : NotFound();
        });

    [HttpDelete("ingresos/{id:int}")]
    public Task<IActionResult> EliminarIngreso(int id) =>
        Ejecutar(async () =>
            await _ingresos.EliminarAsync(id) ? NoContent() : NotFound());

    private static void ValidarAutomovil(AutomovilEntrada datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Color) ||
            string.IsNullOrWhiteSpace(datos.Fabricante) ||
            datos.Anio < 1900 ||
            datos.Anio > DateTime.Today.Year + 1 ||
            !new[] { "Sedan", "4x4", "Moto" }
                .Contains(datos.Tipo, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Datos del automovil invalidos.");
        }
    }

    private static void ValidarParqueo(ParqueoEntrada datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Provincia) ||
            string.IsNullOrWhiteSpace(datos.Nombre) ||
            datos.PrecioPorHora <= 0)
        {
            throw new ArgumentException("Datos del parqueo invalidos.");
        }
    }

    private async Task ValidarIngresoAsync(IngresoEntrada datos)
    {
        if (datos.FechaHoraEntrada == default ||
            (datos.FechaHoraSalida.HasValue &&
             datos.FechaHoraSalida.Value < datos.FechaHoraEntrada))
        {
            throw new ArgumentException("Fechas del ingreso invalidas.");
        }

        if (await _autos.ObtenerPorIdAsync(datos.IdAutomovil) == null ||
            await _parqueos.ObtenerPorIdAsync(datos.IdParqueo) == null)
        {
            throw new ArgumentException(
                "El automovil o el parqueo no existe.");
        }
    }

    private async Task<IActionResult> Ejecutar(
        Func<Task<IActionResult>> accion)
    {
        try
        {
            return await accion();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Error = ex.Message });
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pg &&
                  pg.SqlState is "23503" or "23505" or "23514")
        {
            return Conflict(new
            {
                Error = "La operacion incumple una restriccion de la base de datos."
            });
        }
    }
}

public record AutomovilEntrada(
    string Color, int Anio, string Fabricante, string Tipo);

public record ParqueoEntrada(
    string Provincia, string Nombre, decimal PrecioPorHora);

public record IngresoEntrada(
    int IdParqueo, int IdAutomovil,
    DateTime FechaHoraEntrada, DateTime? FechaHoraSalida);
