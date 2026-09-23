using Utils.DTOs.Comun;

namespace Utils.DTOs.Turno
{
    public class TurnoResponseDto
    {
        public int Id { get; set; }
        public int? TurnoFijoId { get; set; }
        public int PacienteId { get; set; }
        public int ProfesionalId { get; set; }
        public DateTime FechaHora { get; set; }
        public int DuracionMin { get; set; }
        public string Estado { get; set; } = string.Empty;
        public bool Justificada { get; set; }
        public bool Facturable { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string? Observaciones { get; set; }
        public PacienteResumenDto? Paciente { get; set; }
        public ProfesionalResumenDto? Profesional { get; set; }
    }
}