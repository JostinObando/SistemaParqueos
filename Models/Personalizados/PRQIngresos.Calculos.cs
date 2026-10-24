using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParqueos.Models.Entidades;

public partial class PRQIngresos
{
    [NotMapped]
    public double? DuracionMinutos =>
        FechaHoraSalida.HasValue
            ? (FechaHoraSalida.Value - FechaHoraEntrada).TotalMinutes
            : null;

    [NotMapped]
    public double? DuracionHoras =>
        DuracionMinutos.HasValue
            ? DuracionMinutos.Value / 60
            : null;

    [NotMapped]
    public decimal? MontoTotalPagar
    {
        get
        {
            if (!DuracionHoras.HasValue || IdParqueoNavigation == null)
                return null;

            return (decimal)DuracionHoras.Value
                * IdParqueoNavigation.PrecioPorHora;
        }
    }
}
