using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

[Table("Proveedor")]
public partial class Proveedor
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    [StringLength(255)]
    public string Nombre { get; set; } = null!;

    [Column("direccion")]
    [StringLength(255)]
    public string? Direccion { get; set; }

    [Column("telefono")]
    [StringLength(255)]
    public string? Telefono { get; set; }

    [Column("contacto_correo")]
    [StringLength(255)]
    public string? ContactoCorreo { get; set; }

    [InverseProperty("IdProveedorNavigation")]
    public virtual ICollection<CompraProveedor> CompraProveedors { get; set; } = new List<CompraProveedor>();
}
