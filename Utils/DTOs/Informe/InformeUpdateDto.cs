using System.ComponentModel.DataAnnotations;

namespace Utils.DTOs.Informe
{
    public class InformeUpdateDto
    {
        public int? PacienteId { get; set; }

        public int? TurnoId { get; set; }

        [Required]
        public int ProfesionalId { get; set; }

        [Required]
        public DateOnly Fecha { get; set; }

        [MaxLength(50)]
        public string? Tipo { get; set; }

        public string? Contenido { get; set; }

        [MaxLength(20)]
        public string? Estado { get; set; }
    }
}
