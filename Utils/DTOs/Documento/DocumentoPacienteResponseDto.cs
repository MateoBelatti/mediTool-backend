namespace Utils.DTOs.Documento
{
    public class DocumentoPacienteResponseDto
    {
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public int ProfesionalId { get; set; }
        public string? ProfesionalNombre { get; set; }
        public string NombreOriginal { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long TamanoBytes { get; set; }
        public DateTime FechaSubida { get; set; }
    }
}
