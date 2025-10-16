using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

[Table("Torneo")]
public partial class Torneo
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    [StringLength(255)]
    public string Nombre { get; set; } = null!;

    [Column("fecha")]
    public DateOnly Fecha { get; set; }

    [Column("entrada")]
    [Precision(18, 2)]
    public decimal Entrada { get; set; }

    [Column("estado")]
    [StringLength(50)]
    public string Estado { get; set; } = "En Curso";  // Valor por defecto

    [InverseProperty("IdTorneoNavigation")]
    public virtual ICollection<TorneoCliente> TorneoClientes { get; set; } = new List<TorneoCliente>();
}
