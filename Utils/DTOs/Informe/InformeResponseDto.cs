namespace Utils.DTOs.Informe
{
    public class InformeResponseDto
    {
        public int Id { get; set; }
        public int? PacienteId { get; set; }
        public int ProfesionalId { get; set; }
        public int? TurnoId { get; set; }
        public DateOnly Fecha { get; set; }
        public string? Tipo { get; set; }
        public string? Contenido { get; set; }
        public string? Estado { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
