using System.ComponentModel.DataAnnotations;

namespace Utils.DTOs.Profesional
{
    public class ProfesionalUpdateDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres.")]
        public string Apellido { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "La prestación no puede superar los 100 caracteres.")]
        public string? Prestacion { get; set; }

        [MaxLength(50, ErrorMessage = "La matrícula no puede superar los 50 caracteres.")]
        public string? Matricula { get; set; }

        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [MaxLength(150, ErrorMessage = "El email no puede superar los 150 caracteres.")]
        public string? Email { get; set; }

        [MaxLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres.")]
        public string? Telefono { get; set; }

        [MaxLength(45, ErrorMessage = "El registro de prestadores no puede superar los 45 caracteres.")]
        public string? RegistroPrestadores { get; set; }
    }
}
