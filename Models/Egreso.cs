using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

[Table("Egreso")]
public partial class Egreso
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_tipo_egreso")]
    public int IdTipoEgreso { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("monto", TypeName = "decimal(18, 2)")]
    public decimal Monto { get; set; }

    [Column("fecha")]
    public DateOnly? Fecha { get; set; }

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [ForeignKey("IdTipoEgreso")]
    [InverseProperty("Egresos")]
    public virtual TipoEgreso IdTipoEgresoNavigation { get; set; } = null!;

    [ForeignKey("IdUsuario")]
    [InverseProperty("Egresos")]
    public virtual User IdUsuarioNavigation { get; set; } = null!;
}
