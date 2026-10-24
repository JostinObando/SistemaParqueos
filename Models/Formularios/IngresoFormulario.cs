using System.ComponentModel.DataAnnotations;

namespace SistemaParqueos.Models.Formularios;

public class IngresoFormulario
{
    public int Consecutivo { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un automovil.")]
    public int IdAutomovil { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un parqueo.")]
    public int IdParqueo { get; set; }

    public DateTime FechaHoraEntrada { get; set; } = DateTime.Now;

    public DateTime? FechaHoraSalida { get; set; }
}
