using System.ComponentModel.DataAnnotations;

namespace Utils.DTOs.Turno
{
    public class CrearTurnoDto
    {
        [Required]
        public int? TurnoFijoId { get; set; }

        [Required]
        public int PacienteId { get; set; }

        [Required]
        public int ProfesionalId { get; set; }

        [Required]
        public DateTime FechaHora { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "DuracionMin debe ser mayor a 0.")]
        public int DuracionMin { get; set; }

        public EstadoTurno Estado { get; set; } = EstadoTurno.Pendiente;

        public bool Justificada { get; set; }
        public bool Facturable { get; set; }
        public string? Observaciones { get; set; }
    }
}
