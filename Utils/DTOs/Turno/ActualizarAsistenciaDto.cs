namespace Utils.DTOs.Turno
{
    public class ActualizarAsistenciaDto
    {
        public bool Asistio { get; set; }
        public bool Justificada { get; set; }
        [System.ComponentModel.DataAnnotations.MaxLength(500)]
        public string? Observaciones { get; set; }
    }
}