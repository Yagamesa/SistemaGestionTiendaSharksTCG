using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

[Keyless]
public partial class ReporteClientesDestacado
{
    [StringLength(511)]
    public string? Cliente { get; set; }

    public int? TotalCompras { get; set; }

    public int? TorneosParticipados { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? DeudaActual { get; set; }

    public long? RankingCompras { get; set; }

    public long? RankingTorneos { get; set; }

    public long? RankingDeudas { get; set; }
}
