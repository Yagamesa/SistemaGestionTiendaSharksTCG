using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

[Table("Ingreso")]
public partial class Ingreso
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_tipo_ingreso")]
    public int IdTipoIngreso { get; set; }

    [Column("id_cliente")]
    public int IdCliente { get; set; }

    [Column("monto", TypeName = "decimal(18, 2)")]
    public decimal Monto { get; set; }

    [Column("fecha")]
    public DateOnly? Fecha { get; set; }

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [ForeignKey("IdCliente")]
    [InverseProperty("Ingresos")]
    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    [ForeignKey("IdTipoIngreso")]
    [InverseProperty("Ingresos")]
    public virtual TipoIngreso IdTipoIngresoNavigation { get; set; } = null!;
}
