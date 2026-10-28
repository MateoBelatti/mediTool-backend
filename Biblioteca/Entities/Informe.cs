using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biblioteca.Entities
{
    [Table("informe")]
    public class Informe
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("paciente_id")]
        public int? PacienteId { get; set; }

        [Required]
        [Column("profesional_id")]
        public int ProfesionalId { get; set; }

        [Required]
        [Column("fecha")]
        public DateOnly Fecha { get; set; }

        [MaxLength(50)]
        [Column("tipo")]
        public string? Tipo { get; set; }

        [Column("contenido", TypeName = "text")]
        public string? Contenido { get; set; }

        [MaxLength(20)]
        [Column("estado")]
        public string? Estado { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("turno_id")]
        public int? TurnoId { get; set; }

        // Navigation properties
        public virtual Paciente? Paciente { get; set; }
        public virtual Profesional? Profesional { get; set; }
        public virtual Turno? Turno { get; set; }
    }
}
