using System.ComponentModel.DataAnnotations;

namespace Utils.DTOs.Reunion
{
    public class ReunionUpdateDto
    {
        [Required]
        [MaxLength(150)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public DateTime FechaHora { get; set; }

        [MaxLength(20)]
        public string? Modalidad { get; set; }

        public string? Descripcion { get; set; }

        [Required]
        public int ProfesionalId { get; set; }
    }
}
