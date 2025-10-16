using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

public partial class Categoria
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    [StringLength(255)]
    public string Nombre { get; set; } = null!;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [InverseProperty("IdCategoriaNavigation")]
    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
