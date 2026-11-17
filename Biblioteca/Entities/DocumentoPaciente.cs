using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biblioteca.Entities
{
    [Table("documento_paciente")]
    public class DocumentoPaciente
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("paciente_id")]
        public int PacienteId { get; set; }

        [Required]
        [Column("profesional_id")]
        public int ProfesionalId { get; set; }

        [Required]
        [MaxLength(36)]
        [Column("archivo_clave")]
        public string ArchivoClave { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("nombre_original")]
        public string NombreOriginal { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        [Column("extension")]
        public string Extension { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("content_type")]
        public string ContentType { get; set; } = string.Empty;

        [Column("tamano_bytes")]
        public long TamanoBytes { get; set; }

        [Column("fecha_subida")]
        public DateTime FechaSubida { get; set; }

        public virtual Paciente? Paciente { get; set; }
        public virtual Profesional? Profesional { get; set; }
        public virtual Archivo? Archivo { get; set; }
    }
}
