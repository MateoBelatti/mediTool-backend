using System.ComponentModel.DataAnnotations;

namespace Utils.DTOs.Paciente
{
    public class PacienteUpdateDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres.")]
        public string Apellido { get; set; } = string.Empty;

        public DateOnly? FechaNacimiento { get; set; }

        [MaxLength(20, ErrorMessage = "El DNI no puede superar los 20 caracteres.")]
        public string? Dni { get; set; }

        [MaxLength(50, ErrorMessage = "La dirección no puede superar los 50 caracteres.")]
        public string? Direccion { get; set; }

        [MaxLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres.")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [MaxLength(150, ErrorMessage = "El email no puede superar los 150 caracteres.")]
        public string? Email { get; set; }

        [MaxLength(100, ErrorMessage = "La obra social no puede superar los 100 caracteres.")]
        public string? ObraSocial { get; set; }

        [MaxLength(50, ErrorMessage = "El número de afiliado no puede superar los 50 caracteres.")]
        public string? NroAfiliado { get; set; }

        [MaxLength(100, ErrorMessage = "La escuela no puede superar los 100 caracteres.")]
        public string? Escuela { get; set; }
    }
}
