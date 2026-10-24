using System;
using System.Collections.Generic;

namespace SistemaParqueos.Models.Entidades;

public partial class PRQIngresos
{
    public int Consecutivo { get; set; }

    public int IdParqueo { get; set; }

    public int IdAutomovil { get; set; }

    public DateTime FechaHoraEntrada { get; set; }

    public DateTime? FechaHoraSalida { get; set; }

    public virtual PRQAutomoviles IdAutomovilNavigation { get; set; } = null!;

    public virtual PRQParqueos IdParqueoNavigation { get; set; } = null!;
}
