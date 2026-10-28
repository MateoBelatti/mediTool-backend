using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biblioteca.Entities
{
    [Table("turno_fijo")]
    public class TurnoFijo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("paciente_id")]
        public int PacienteId { get; set; }

        [Required]
        [Column("profesional_id")]
        public int ProfesionalId { get; set; }

        [Required]
        [Column("dia_semana")]
        public int DiaSemana { get; set; }

        [Required]
        [Column("hora")]
        public TimeSpan Hora { get; set; }

        [Required]
        [Column("duracion_min")]
        public int DuracionMin { get; set; }

        [Required]
        [Column("fecha_inicio")]
        public DateOnly FechaInicio { get; set; }

        [Column("fecha_fin")]
        public DateOnly? FechaFin { get; set; }

        [Required]
        [Column("activo")]
        public bool Activo { get; set; }

        // Navigation properties
        [ForeignKey("PacienteId")]
        public Paciente? Paciente { get; set; }

        [ForeignKey("ProfesionalId")]
        public Profesional? Profesional { get; set; }

        public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
    }
}
