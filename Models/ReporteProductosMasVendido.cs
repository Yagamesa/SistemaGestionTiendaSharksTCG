using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

[Keyless]
public partial class ReporteProductosMasVendido
{
    [StringLength(255)]
    public string Producto { get; set; } = null!;

    [StringLength(255)]
    public string Categoria { get; set; } = null!;

    public int? UnidadesVendidas { get; set; }

    [Column(TypeName = "decimal(38, 2)")]
    public decimal? GananciaTotal { get; set; }

    public long? RankingUnidades { get; set; }

    public long? RankingGanancias { get; set; }

    public long? RankingCategoria { get; set; }
}
