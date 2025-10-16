using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

public partial class Preventum
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_cliente")]
    public int IdCliente { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("fecha_preventa")]
    public DateOnly? FechaPreventa { get; set; }

    [Column("total", TypeName = "decimal(18, 2)")]
    public decimal Total { get; set; }

    [Column("tipo_pago")]
    [StringLength(50)]
    public string TipoPago { get; set; } = null!;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [ForeignKey("IdCliente")]
    [InverseProperty("Preventa")]
    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    [ForeignKey("IdUsuario")]
    [InverseProperty("Preventa")]
    public virtual User IdUsuarioNavigation { get; set; } = null!;
}
