using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

[PrimaryKey("IdTorneo", "IdCliente")]
[Table("Torneo_Cliente")]
public partial class TorneoCliente
{
    [Key]
    [Column("id_torneo")]
    public int IdTorneo { get; set; }

    [Key]
    [Column("id_cliente")]
    public int IdCliente { get; set; }

    [Column("pago", TypeName = "decimal(18, 2)")]
    public decimal Pago { get; set; }

    [Column("tipo_pago")]
    [StringLength(50)]
    public string TipoPago { get; set; } = null!;

    [InverseProperty("TorneoCliente")]
    public virtual ICollection<GanadoresTorneo> GanadoresTorneos { get; set; } = new List<GanadoresTorneo>();

    [ForeignKey("IdCliente")]
    [InverseProperty("TorneoClientes")]
    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    [ForeignKey("IdTorneo")]
    [InverseProperty("TorneoClientes")]
    public virtual Torneo IdTorneoNavigation { get; set; } = null!;
}
