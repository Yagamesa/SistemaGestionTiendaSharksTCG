using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

[Table("Producto")]
public partial class Producto
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    [StringLength(255)]
    public string Nombre { get; set; } = null!;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("id_categoria")]
    public int IdCategoria { get; set; }

    [Column("precio_compra", TypeName = "decimal(18, 2)")]
    public decimal? PrecioCompra { get; set; }

    [Column("precio_venta", TypeName = "decimal(18, 2)")]
    public decimal? PrecioVenta { get; set; }

    [Column("precio_sharkcoins", TypeName = "decimal(18, 2)")]
    public decimal? PrecioSharkcoins { get; set; }

    [ForeignKey("IdCategoria")]
    [InverseProperty("Productos")]
    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

    [InverseProperty("IdProductoNavigation")]
    public virtual ICollection<ProductoVenta> ProductoVenta { get; set; } = new List<ProductoVenta>();

    [InverseProperty("IdProductoNavigation")]
    public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
}
