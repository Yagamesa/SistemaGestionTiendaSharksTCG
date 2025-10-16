using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

[Table("Rol")]
public partial class Rol
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    [StringLength(255)]
    public string Nombre { get; set; } = null!;

    [Column("permisos")]
    public string? Permisos { get; set; }

    [InverseProperty("IdRolNavigation")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    // ✅ Propiedad de navegación renombrada para evitar conflicto
    [InverseProperty("Rol")]
    public virtual ICollection<Permiso> PermisosAsignados { get; set; } = new List<Permiso>();


}
