using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

[Table("Stock")]
public partial class Stock
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_producto")]
    public int IdProducto { get; set; }

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("tipo_movimiento")]
    [StringLength(20)]
    public string? TipoMovimiento { get; set; }

    [Column("fecha_movimiento")]
    public DateTime? FechaMovimiento { get; set; }

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [ForeignKey("IdProducto")]
    [InverseProperty("Stocks")]
    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
