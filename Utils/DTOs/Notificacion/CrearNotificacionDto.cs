using System.ComponentModel.DataAnnotations;
using Utils.Enums;

namespace Utils.DTOs.Notificacion
{
    public class CrearNotificacionDto
    {
        [Required]
        public int ProfesionalId { get; set; }

        [Required]
        public TipoNotificacion Tipo { get; set; }

        [Required]
        [MaxLength(150, ErrorMessage = "El título no puede superar los 150 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [MaxLength(500, ErrorMessage = "El mensaje no puede superar los 500 caracteres.")]
        public string Mensaje { get; set; } = string.Empty;
    }
}
