using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biblioteca.Entities
{
    [Table("archivo")]
    public class Archivo
    {
        [Key]
        [MaxLength(36)]
        [Column("clave")]
        public string Clave { get; set; } = string.Empty;

        [Required]
        [Column("contenido", TypeName = "bytea")]
        public byte[] Contenido { get; set; } = Array.Empty<byte>();

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
