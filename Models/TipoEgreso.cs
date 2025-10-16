using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

[Table("tipo_egreso")]
public partial class TipoEgreso
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    [StringLength(255)]
    public string Nombre { get; set; } = null!;

    [InverseProperty("IdTipoEgresoNavigation")]
    public virtual ICollection<Egreso> Egresos { get; set; } = new List<Egreso>();
}
