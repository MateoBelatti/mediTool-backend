namespace Utils.DTOs.TurnoFijo
{
    public class EditarTurnoFijoDto
    {
        public int DiaSemana { get; set; }
        public TimeSpan Hora { get; set; }
        public int DuracionMin { get; set; }
        public DateOnly? FechaFin { get; set; }
        public bool Activo { get; set; }
    }
}
