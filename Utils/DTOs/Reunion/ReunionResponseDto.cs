namespace Utils.DTOs.Reunion
{
    public class ReunionResponseDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public string? Modalidad { get; set; }
        public string? Descripcion { get; set; }
        public int ProfesionalId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
