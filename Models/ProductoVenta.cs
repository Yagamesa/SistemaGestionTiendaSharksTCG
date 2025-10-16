using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

[PrimaryKey("IdProducto", "IdVenta")]
[Table("Producto_Venta")]
public partial class ProductoVenta
{
    [Key]
    [Column("id_producto")]
    public int IdProducto { get; set; }

    [Key]
    [Column("id_venta")]
    public int IdVenta { get; set; }

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("precio_unitario", TypeName = "decimal(18, 2)")]
    public decimal PrecioUnitario { get; set; }

    [Column("tipo_pago")]
    [StringLength(50)]
    public string TipoPago { get; set; } = null!;

    [ForeignKey("IdProducto")]
    [InverseProperty("ProductoVenta")]
    public virtual Producto IdProductoNavigation { get; set; } = null!;

    [ForeignKey("IdVenta")]
    [InverseProperty("ProductoVenta")]
    public virtual Venta IdVentaNavigation { get; set; } = null!;
}
