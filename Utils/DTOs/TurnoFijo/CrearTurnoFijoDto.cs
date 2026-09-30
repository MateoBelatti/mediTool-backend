namespace Utils.DTOs.TurnoFijo
{
    public class CrearTurnoFijoDto
    {
        public int PacienteId { get; set; }
        public int ProfesionalId { get; set; }
        public int DiaSemana { get; set; }
        public TimeSpan Hora { get; set; }
        public int DuracionMin { get; set; }
        public DateOnly FechaInicio { get; set; }
        public DateOnly? FechaFin { get; set; }
        public bool Activo { get; set; } = true;
        public int? HorizonteMeses { get; set; }
    }
}
