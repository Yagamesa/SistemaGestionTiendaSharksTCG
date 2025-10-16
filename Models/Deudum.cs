using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

public partial class Deudum
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_cliente")]
    public int IdCliente { get; set; }

    [Column("monto", TypeName = "decimal(18, 2)")]
    public decimal Monto { get; set; }

    [Column("tipoDeuda")]
    [StringLength(255)]
    public string? TipoDeuda { get; set; }

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("IdCliente")]
    [InverseProperty("DeudaNavigation")]
    public virtual Cliente IdClienteNavigation { get; set; } = null!;
}
