using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

[Table("Sharkcoin")]
public partial class Sharkcoin
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_cliente")]
    public int IdCliente { get; set; }

    [Column("monto", TypeName = "decimal(18, 2)")]
    public decimal Monto { get; set; }

    [Column("tipo")]
    [StringLength(255)]
    public string? Tipo { get; set; }

    [Column("fecha")]
    public DateTime? Fecha { get; set; }

    [ForeignKey("IdCliente")]
    [InverseProperty("Sharkcoins")]
    public virtual Cliente IdClienteNavigation { get; set; } = null!;
}
