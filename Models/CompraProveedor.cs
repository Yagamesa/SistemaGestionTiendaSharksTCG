using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

[PrimaryKey("IdCompra", "IdProveedor")]
[Table("Compra_Proveedor")]
public partial class CompraProveedor
{
    [Key]
    [Column("id_compra")]
    public int IdCompra { get; set; }

    [Key]
    [Column("id_proveedor")]
    public int IdProveedor { get; set; }

    [Column("nombre_producto")]
    [StringLength(255)]
    public string NombreProducto { get; set; } = null!;

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("precio_unitario", TypeName = "decimal(18, 2)")]
    public decimal PrecioUnitario { get; set; }

    [ForeignKey("IdCompra")]
    [InverseProperty("CompraProveedors")]
    public virtual Compra IdCompraNavigation { get; set; } = null!;

    [ForeignKey("IdProveedor")]
    [InverseProperty("CompraProveedors")]
    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;
}
