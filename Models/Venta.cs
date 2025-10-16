using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

public partial class Venta
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_cliente")]
    public int IdCliente { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("fecha_venta")]
    public DateOnly? FechaVenta { get; set; }

    [Column("total")]
    public decimal Total { get; set; }

    [Column("descuento")]
    public decimal Descuento { get; set; }

    [Column("pago")]
    public decimal Pago { get; set; }

    [ForeignKey("IdCliente")]
    [InverseProperty("Venta")]
    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    [ForeignKey("IdUsuario")]
    [InverseProperty("Venta")]
    public virtual User IdUsuarioNavigation { get; set; } = null!;

    [InverseProperty("IdVentaNavigation")]
    public virtual ICollection<ProductoVenta> ProductoVenta { get; set; } = new List<ProductoVenta>();
}
