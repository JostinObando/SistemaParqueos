using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using SistemaParqueos.Models.Formularios;

namespace SistemaParqueos.Controllers;

public class AutomovilesMantenimientoController : Controller
{
    private readonly IHttpClientFactory _clientes;

    public AutomovilesMantenimientoController(
        IHttpClientFactory clientes)
    {
        _clientes = clientes;
    }

    private HttpClient CrearCliente()
    {
        var cliente = _clientes.CreateClient();
        cliente.BaseAddress = new Uri(
            $"{Request.Scheme}://{Request.Host}/");
        return cliente;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int? anioInicio, int? anioFin)
    {
        ViewBag.AnioInicio = anioInicio;
        ViewBag.AnioFin = anioFin;

        if (anioInicio.HasValue && anioFin.HasValue &&
            anioInicio > anioFin)
        {
            ViewBag.Error = "El año inicial no puede superar al final.";
            return View(new List<AutomovilMantenimientoModelo>());
        }

        var parametros = new List<string>();

        if (anioInicio.HasValue)
            parametros.Add($"anioInicio={anioInicio.Value}");

        if (anioFin.HasValue)
            parametros.Add($"anioFin={anioFin.Value}");

        var ruta = "api/automoviles";

        if (parametros.Count > 0)
            ruta += "?" + string.Join("&", parametros);

        try
        {
            using var cliente = CrearCliente();
            var datos = await cliente.GetFromJsonAsync<
                List<AutomovilMantenimientoModelo>>(ruta);

            return View(datos ?? new());
        }
        catch (Exception)
        {
            ViewBag.Error = "No se pudo consultar la API de automoviles.";
            return View(new List<AutomovilMantenimientoModelo>());
        }
    }

    [HttpGet]
    public IActionResult Crear()
    {
        return View("Formulario",
            new AutomovilMantenimientoModelo());
    }

    [HttpGet]
    public async Task<IActionResult> Detalles(int id)
    {
        using var cliente = CrearCliente();
        var respuesta = await cliente.GetAsync($"api/automoviles/{id}");

        if (respuesta.StatusCode == HttpStatusCode.NotFound)
            return NotFound();

        if (!respuesta.IsSuccessStatusCode)
            return StatusCode(502);

        var modelo = await respuesta.Content.ReadFromJsonAsync<
            AutomovilMantenimientoModelo>();

        return modelo == null ? NotFound() : View(modelo);
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        using var cliente = CrearCliente();
        var respuesta = await cliente.GetAsync($"api/automoviles/{id}");

        if (respuesta.StatusCode == HttpStatusCode.NotFound)
            return NotFound();

        if (!respuesta.IsSuccessStatusCode)
            return StatusCode(502);

        var modelo = await respuesta.Content.ReadFromJsonAsync<
            AutomovilMantenimientoModelo>();

        return modelo == null
            ? NotFound()
            : View("Formulario", modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Guardar(
        AutomovilMantenimientoModelo modelo)
    {
        if (modelo.Anio > DateTime.Now.Year + 1)
            ModelState.AddModelError("Anio", "El año no es valido.");

        if (!ModelState.IsValid)
            return View("Formulario", modelo);

        try
        {
            using var cliente = CrearCliente();

            var datos = new
            {
                modelo.Color,
                modelo.Anio,
                modelo.Fabricante,
                modelo.Tipo
            };

            HttpResponseMessage respuesta;

            if (modelo.Id == 0)
                respuesta = await cliente.PostAsJsonAsync(
                    "api/automoviles", datos);
            else
                respuesta = await cliente.PutAsJsonAsync(
                    $"api/automoviles/{modelo.Id}", datos);

            using (respuesta)
            {
                if (!respuesta.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(
                        "", "La API no pudo guardar el automovil.");
                    return View("Formulario", modelo);
                }
            }

            TempData["Exito"] = "Automovil guardado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            ModelState.AddModelError(
                "", "No fue posible comunicarse con la API.");
            return View("Formulario", modelo);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            using var cliente = CrearCliente();
            using var respuesta = await cliente.DeleteAsync(
                $"api/automoviles/{id}");

            TempData[respuesta.IsSuccessStatusCode
                ? "Exito" : "Error"] =
                respuesta.IsSuccessStatusCode
                ? "Automovil eliminado."
                : "No se pudo eliminar. Revise si tiene ingresos asociados.";
        }
        catch (Exception)
        {
            TempData["Error"] = "No se pudo comunicar con la API.";
        }

        return RedirectToAction(nameof(Index));
    }
}
