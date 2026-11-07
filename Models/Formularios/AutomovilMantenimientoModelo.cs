using System.ComponentModel.DataAnnotations;

namespace SistemaParqueos.Models.Formularios;

public class AutomovilMantenimientoModelo
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ingrese el color.")]
    public string Color { get; set; } = "";

    [Range(1900, 2100, ErrorMessage = "El año no es valido.")]
    public int Anio { get; set; } = DateTime.Now.Year;

    [Required(ErrorMessage = "Ingrese el fabricante.")]
    public string Fabricante { get; set; } = "";

    [Required(ErrorMessage = "Seleccione el tipo.")]
    public string Tipo { get; set; } = "";
}
