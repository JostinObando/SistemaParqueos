using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaParqueos.Models.Entidades;
using SistemaParqueos.Models.Formularios;
using SistemaParqueos.Repositories.Interfaces;

namespace SistemaParqueos.Controllers;

public class IngresosController : Controller
{
    private readonly IIngresoRepository _ingresos;
    private readonly IAutomovilRepository _autos;
    private readonly IParqueoRepository _parqueos;

    public IngresosController(
        IIngresoRepository ingresos,
        IAutomovilRepository autos,
        IParqueoRepository parqueos)
    {
        _ingresos = ingresos;
        _autos = autos;
        _parqueos = parqueos;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var registros = await _ingresos.ConsultarPorProvinciaAsync(
            "", DateTime.MinValue, DateTime.MaxValue);

        return View(registros
            .OrderByDescending(x => x.FechaHoraEntrada)
            .ToList());
    }

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        await CargarOpciones();
        return View("Formulario", new IngresoFormulario());
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var ingreso = await _ingresos.ObtenerPorIdAsync(id);

        if (ingreso == null)
            return NotFound();

        await CargarOpciones();

        return View("Formulario", new IngresoFormulario
        {
            Consecutivo = ingreso.Consecutivo,
            IdAutomovil = ingreso.IdAutomovil,
            IdParqueo = ingreso.IdParqueo,
            FechaHoraEntrada = ingreso.FechaHoraEntrada,
            FechaHoraSalida = ingreso.FechaHoraSalida
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Guardar(IngresoFormulario modelo)
    {
        if (modelo.FechaHoraEntrada == default ||
            (modelo.FechaHoraSalida.HasValue &&
             modelo.FechaHoraSalida < modelo.FechaHoraEntrada))
        {
            ModelState.AddModelError(
                "", "Las fechas del ingreso no son validas.");
        }

        if (await _autos.ObtenerPorIdAsync(modelo.IdAutomovil) == null ||
            await _parqueos.ObtenerPorIdAsync(modelo.IdParqueo) == null)
        {
            ModelState.AddModelError(
                "", "El automovil o parqueo no existe.");
        }

        if (!ModelState.IsValid)
        {
            await CargarOpciones();
            return View("Formulario", modelo);
        }

        var entidad = new PRQIngresos
        {
            Consecutivo = modelo.Consecutivo,
            IdAutomovil = modelo.IdAutomovil,
            IdParqueo = modelo.IdParqueo,
            FechaHoraEntrada = modelo.FechaHoraEntrada,
            FechaHoraSalida = modelo.FechaHoraSalida
        };

        try
        {
            if (modelo.Consecutivo == 0)
                await _ingresos.CrearAsync(entidad);
            else if (!await _ingresos.ActualizarAsync(entidad))
                return NotFound();

            TempData["Exito"] = "Ingreso guardado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            ModelState.AddModelError(
                "", "No se pudo guardar el ingreso.");

            await CargarOpciones();
            return View("Formulario", modelo);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        if (!await _ingresos.EliminarAsync(id))
            return NotFound();

        TempData["Exito"] = "Ingreso eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarOpciones()
    {
        var autos = await _autos.ConsultarAsync(
            null, null, null, null, null);

        var parqueos = await _parqueos.ConsultarAsync(
            null, null, null, null);

        ViewBag.Automoviles = new SelectList(
            autos.Select(x => new
            {
                x.Id,
                Descripcion = $"{x.Fabricante} - {x.Tipo} ({x.Id})"
            }),
            "Id", "Descripcion");

        ViewBag.Parqueos = new SelectList(
            parqueos.Select(x => new
            {
                x.Id,
                Descripcion = $"{x.Nombre} - ₡{x.PrecioPorHora:N0}/h"
            }),
            "Id", "Descripcion");
    }
}
