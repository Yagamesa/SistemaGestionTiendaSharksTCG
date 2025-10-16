using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

[Index("Email", Name = "UQ__Users__AB6E61641D7670AF", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    [StringLength(255)]
    public string Nombre { get; set; } = null!;

    [Column("apellido_paterno")]
    [StringLength(255)]
    public string? ApellidoPaterno { get; set; }

    [Column("apellido_materno")]
    [StringLength(255)]
    public string? ApellidoMaterno { get; set; }

    [Column("id_rol")]
    public int IdRol { get; set; }

    [Column("email")]
    [StringLength(255)]
    public string Email { get; set; } = null!;

    [Column("password")]
    [StringLength(255)]
    public string Password { get; set; } = null!;

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Egreso> Egresos { get; set; } = new List<Egreso>();

    [ForeignKey("IdRol")]
    [InverseProperty("Users")]
    public virtual Rol IdRolNavigation { get; set; } = null!;

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Preventum> Preventa { get; set; } = new List<Preventum>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
