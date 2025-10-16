using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

[Table("Cliente")]
[Index("Ci", Name = "UQ__Cliente__3213666237890830", IsUnique = true)]
public partial class Cliente
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

    [Column("ci")]
    [StringLength(255)]
    public string? Ci { get; set; } //= null!; //quitar comentario  y quitar el ? de string?
                                    //para que salga required en el form

    [Column("celular")]
    [StringLength(255)]
    public string? Celular { get; set; }

    [Column("sharkCoins", TypeName = "decimal(18, 2)")]
    public decimal? SharkCoins { get; set; }

    [Column("deuda", TypeName = "decimal(18, 2)")]
    public decimal? Deuda { get; set; }

    [InverseProperty("IdClienteNavigation")]
    public virtual ICollection<CodigoTcg> CodigoTcgs { get; set; } = new List<CodigoTcg>();

    [InverseProperty("IdClienteNavigation")]
    public virtual ICollection<Deudum> DeudaNavigation { get; set; } = new List<Deudum>();

    [InverseProperty("IdClienteNavigation")]
    public virtual ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();

    [InverseProperty("IdClienteNavigation")]
    public virtual ICollection<Preventum> Preventa { get; set; } = new List<Preventum>();

    [InverseProperty("IdClienteNavigation")]
    public virtual ICollection<Sharkcoin> Sharkcoins { get; set; } = new List<Sharkcoin>();
    [JsonIgnore] //
    [InverseProperty("IdClienteNavigation")]
    public virtual ICollection<TorneoCliente> TorneoClientes { get; set; } = new List<TorneoCliente>();

    [InverseProperty("IdClienteNavigation")]
    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
