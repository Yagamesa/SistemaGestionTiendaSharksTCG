using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

[Table("tipo_ingreso")]
public partial class TipoIngreso
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    [StringLength(255)]
    public string Nombre { get; set; } = null!;

    [InverseProperty("IdTipoIngresoNavigation")]
    public virtual ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
}
