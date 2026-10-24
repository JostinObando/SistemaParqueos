using Microsoft.AspNetCore.Mvc;
using SistemaParqueos.Models.Entidades;
using SistemaParqueos.Repositories.Interfaces;

namespace SistemaParqueos.Controllers;

public class ReportesController : Controller
{
    private readonly IIngresoRepository _repo;

    public ReportesController(IIngresoRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? criterio, string? valor,
        DateTime? desde, DateTime? hasta)
    {
        ViewBag.Criterio = criterio;
        ViewBag.Valor = valor;
        ViewBag.Desde = desde;
        ViewBag.Hasta = hasta;

        var resultado = new List<PRQIngresos>();

        if (!string.IsNullOrWhiteSpace(valor))
        {
            var inicio = desde ?? new DateTime(2000, 1, 1);

            var fin = hasta?.Date.AddDays(1).AddTicks(-1)
                ?? new DateTime(2100, 1, 1);

            if (inicio > fin)
            {
                ViewBag.Error =
                    "La fecha inicial debe ser anterior a la final.";
            }
            else if (criterio == "provincia")
            {
                resultado = await _repo.ConsultarPorProvinciaAsync(
                    valor, inicio, fin);
            }
            else
            {
                resultado = await _repo.ConsultarPorTipoAsync(
                    valor, inicio, fin);
            }
        }

        return View(resultado);
    }
}
