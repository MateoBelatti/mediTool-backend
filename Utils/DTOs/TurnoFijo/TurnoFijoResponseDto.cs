using Utils.DTOs.Comun;

namespace Utils.DTOs.TurnoFijo
{
    public class TurnoFijoResponseDto
    {
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public int ProfesionalId { get; set; }
        public int DiaSemana { get; set; }
        public TimeSpan Hora { get; set; }
        public int DuracionMin { get; set; }
        public DateOnly FechaInicio { get; set; }
        public DateOnly? FechaFin { get; set; }
        public bool Activo { get; set; }
        public PacienteResumenDto? Paciente { get; set; }
        public ProfesionalResumenDto? Profesional { get; set; }
    }
}