using Microsoft.AspNetCore.Mvc;
using SistemaParqueos.Models.Entidades;
using SistemaParqueos.Repositories.Interfaces;

namespace SistemaParqueos.Controllers;

public class ParqueosController : Controller
{
    private readonly IParqueoRepository _repo;

    public ParqueosController(IParqueoRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? provincia, string? nombre,
        decimal? precioMinimo, decimal? precioMaximo)
    {
        ViewBag.Provincia = provincia;
        ViewBag.Nombre = nombre;
        ViewBag.PrecioMinimo = precioMinimo;
        ViewBag.PrecioMaximo = precioMaximo;

        return View(await _repo.ConsultarAsync(
            provincia, nombre, precioMinimo, precioMaximo));
    }

    [HttpGet]
    public IActionResult Crear()
    {
        return View("Formulario", new PRQParqueos());
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
    public async Task<IActionResult> Guardar(PRQParqueos modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo.Nombre) ||
            string.IsNullOrWhiteSpace(modelo.Provincia) ||
            modelo.PrecioPorHora <= 0)
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

            TempData["Exito"] = "Parqueo guardado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            ModelState.AddModelError(
                "", "No se pudo guardar el parqueo.");

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

            TempData["Exito"] = "Parqueo eliminado.";
        }
        catch (Exception)
        {
            TempData["Error"] =
                "No se puede eliminar un parqueo con ingresos relacionados.";
        }

        return RedirectToAction(nameof(Index));
    }
}
