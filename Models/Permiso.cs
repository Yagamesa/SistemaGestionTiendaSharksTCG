using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoMDGSharksWeb.Models
{
    [Table("Permisos")]
    public class Permiso
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Rol")]
        public int IdRol { get; set; }

        [Required]
        [StringLength(100)]
        public string Modulo { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Accion { get; set; } = null!;

        public bool Permitido { get; set; }

        [InverseProperty("PermisosAsignados")]
        public Rol Rol { get; set; } = null!;
    }

}
