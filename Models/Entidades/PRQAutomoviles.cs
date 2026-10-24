using System;
using System.Collections.Generic;

namespace SistemaParqueos.Models.Entidades;

public partial class PRQAutomoviles
{
    public int Id { get; set; }

    public string Color { get; set; } = null!;

    public int Anio { get; set; }

    public string Fabricante { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public virtual ICollection<PRQIngresos> PRQIngresos { get; set; } = new List<PRQIngresos>();
}
