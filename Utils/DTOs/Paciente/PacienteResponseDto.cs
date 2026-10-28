namespace Utils.DTOs.Paciente
{
    public class PacienteResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public DateOnly? FechaNacimiento { get; set; }
        public string? Dni { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? ObraSocial { get; set; }
        public string? NroAfiliado { get; set; }
        public string? Escuela { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
