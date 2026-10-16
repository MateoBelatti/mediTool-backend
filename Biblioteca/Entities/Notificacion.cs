using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Utils.Enums;

namespace Biblioteca.Entities
{
    [Table("notificacion")]
    public class Notificacion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("profesional_id")]
        public int ProfesionalId { get; set; }

        [Required]
        [Column("tipo")]
        public TipoNotificacion Tipo { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        [Column("mensaje")]
        public string Mensaje { get; set; } = string.Empty;

        [Required]
        [Column("leida")]
        public bool Leida { get; set; }

        [Required]
        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        [ForeignKey("ProfesionalId")]
        public Profesional? Profesional { get; set; }
    }
}
