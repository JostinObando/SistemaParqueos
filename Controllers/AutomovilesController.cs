using Microsoft.AspNetCore.Mvc;
using SistemaParqueos.Models.Entidades;
using SistemaParqueos.Repositories.Interfaces;

namespace SistemaParqueos.Controllers;

public class AutomovilesController : Controller
{
    private readonly IAutomovilRepository _repo;

    public AutomovilesController(IAutomovilRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? color, int? anioInicio, int? anioFin,
        string? fabricante, string? tipo)
    {
        ViewBag.Color = color;
        ViewBag.AnioInicio = anioInicio;
        ViewBag.AnioFin = anioFin;
        ViewBag.Fabricante = fabricante;
        ViewBag.Tipo = tipo;

        return View(await _repo.ConsultarAsync(
            color, anioInicio, anioFin, fabricante, tipo));
    }

    [HttpGet]
    public IActionResult Crear()
    {
        return View("Formulario", new PRQAutomoviles());
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var entidad = await _repo.ObtenerPorIdAsync(id);

        return entidad == null
            ? NotFound()
            : View("Formulario", entidad);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Guardar(PRQAutomoviles modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo.Color) ||
            string.IsNullOrWhiteSpace(modelo.Fabricante) ||
            string.IsNullOrWhiteSpace(modelo.Tipo) ||
            modelo.Anio < 1900 ||
            modelo.Anio > DateTime.Now.Year + 1)
        {
            ModelState.AddModelError(
                "", "Complete correctamente los datos.");
        }

        if (!ModelState.IsValid)
            return View("Formulario", modelo);

        try
        {
            if (modelo.Id == 0)
                await _repo.CrearAsync(modelo);
            else if (!await _repo.ActualizarAsync(modelo))
                return NotFound();

            TempData["Exito"] = "Automovil guardado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            ModelState.AddModelError(
                "", "No se pudo guardar el automovil.");

            return View("Formulario", modelo);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            if (!await _repo.EliminarAsync(id))
                return NotFound();

            TempData["Exito"] = "Automovil eliminado.";
        }
        catch (Exception)
        {
            TempData["Error"] =
                "No se puede eliminar un automovil con ingresos relacionados.";
        }

        return RedirectToAction(nameof(Index));
    }
}
