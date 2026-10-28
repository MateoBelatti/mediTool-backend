namespace Utils.DTOs.Profesional
{
    public class ProfesionalResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string? Prestacion { get; set; }
        public string? Matricula { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? RegistroPrestadores { get; set; }
    }
}
