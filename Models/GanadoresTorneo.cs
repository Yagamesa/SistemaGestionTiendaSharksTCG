using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

[Table("Ganadores_Torneo")]
public partial class GanadoresTorneo
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_torneo")]
    public int IdTorneo { get; set; }

    [Column("id_cliente")]
    public int IdCliente { get; set; }

    [Column("premio_sharkcoins", TypeName = "decimal(18, 2)")]
    public decimal PremioSharkcoins { get; set; }

    [Column("puesto")]
    public int Puesto { get; set; }

    [ForeignKey("IdTorneo, IdCliente")]
    [InverseProperty("GanadoresTorneos")]
    public virtual TorneoCliente TorneoCliente { get; set; } = null!;
}
