namespace Utils.DTOs.Turno
{
    public class ResumenAsistenciaDto
    {
        public int TurnoFijoId { get; set; }
        public int TotalTurnos { get; set; }
        public int TotalAsistencias { get; set; }
        public int TotalAusenciasJustificadas { get; set; }
        public int TotalAusenciasInjustificadas { get; set; }
    }
}