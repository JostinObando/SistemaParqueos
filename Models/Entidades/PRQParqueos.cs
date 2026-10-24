using System;
using System.Collections.Generic;

namespace SistemaParqueos.Models.Entidades;

public partial class PRQParqueos
{
    public int Id { get; set; }

    public string Provincia { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public decimal PrecioPorHora { get; set; }

    public virtual ICollection<PRQIngresos> PRQIngresos { get; set; } = new List<PRQIngresos>();
}
