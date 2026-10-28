using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biblioteca.Entities
{
    [Table("reunion")]
    public class Reunion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [Column("fecha_hora")]
        public DateTime FechaHora { get; set; }

        [MaxLength(20)]
        [Column("modalidad")]
        public string? Modalidad { get; set; }

        [Column("descripcion", TypeName = "text")]
        public string? Descripcion { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Required]
        [Column("profesional_id")]
        public int ProfesionalId { get; set; }

        // Navigation property
        public virtual Profesional? Profesional { get; set; }
    }
}
