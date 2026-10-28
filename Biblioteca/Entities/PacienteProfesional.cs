using System.ComponentModel.DataAnnotations.Schema;

namespace Biblioteca.Entities
{
    [Table("paciente_profesional")]
    public class PacienteProfesional
    {
        [Column("paciente_id")]
        public int PacienteId { get; set; }
        public Paciente Paciente { get; set; } = null!;

        [Column("profesional_id")]
        public int ProfesionalId { get; set; }
        public Profesional Profesional { get; set; } = null!;

        [Column("fecha_vinculacion")]
        public DateTime FechaVinculacion { get; set; } = DateTime.UtcNow;
    }
}
