using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

[Table("Compra")]
public partial class Compra
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("fecha_compra")]
    public DateOnly? FechaCompra { get; set; }

    [InverseProperty("IdCompraNavigation")]
    public virtual ICollection<CompraProveedor> CompraProveedors { get; set; } = new List<CompraProveedor>();

    [ForeignKey("IdUsuario")]
    [InverseProperty("Compras")]
    public virtual User IdUsuarioNavigation { get; set; } = null!;
}
