using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoMDGSharksWeb.Models;

[Table("codigo_tcg")]
public partial class CodigoTcg
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_cliente")]
    public int IdCliente { get; set; }

    [Column("juego")]
    [StringLength(255)]
    public string Juego { get; set; } = null!;

    [Column("codigo")]
    [StringLength(255)]
    public string Codigo { get; set; } = null!;

    [ForeignKey("IdCliente")]
    [InverseProperty("CodigoTcgs")]
    public virtual Cliente IdClienteNavigation { get; set; } = null!;
}
