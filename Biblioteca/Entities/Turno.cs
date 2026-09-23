using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biblioteca.Entities
{
    [Table("turno")]
    public class Turno
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("turno_fijo_id")]
        public int? TurnoFijoId { get; set; }

        [Required]
        [Column("paciente_id")]
        public int PacienteId { get; set; }

        [Required]
        [Column("profesional_id")]
        public int ProfesionalId { get; set; }

        [Required]
        [Column("fecha_hora")]
        public DateTime FechaHora { get; set; }

        [Required]
        [Column("duracion_min")]
        public int DuracionMin { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("estado")]
        public string Estado { get; set; } = string.Empty;

        [Column("justificada")]
        public bool Justificada { get; set; }

        [Column("facturable")]
        public bool Facturable { get; set; }

        [Required]
        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; }

        [Column("observaciones")]
        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // Navigation properties
        [ForeignKey("TurnoFijoId")]
        public TurnoFijo? TurnoFijo { get; set; }

        [ForeignKey("PacienteId")]
        public Paciente? Paciente { get; set; }

        [ForeignKey("ProfesionalId")]
        public Profesional? Profesional { get; set; }

        public ICollection<Asistencia> Asistencias { get; set; } = new List<Asistencia>();
    }
}
