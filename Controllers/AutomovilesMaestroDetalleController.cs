using Microsoft.AspNetCore.Mvc;

namespace SistemaParqueos.Controllers;

public class AutomovilesMaestroDetalleController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
