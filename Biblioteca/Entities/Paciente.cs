using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biblioteca.Entities
{
    [Table("paciente")]
    public class Paciente
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Column("fecha_nacimiento")]
        public DateOnly? FechaNacimiento { get; set; }

        [MaxLength(20)]
        [Column("dni")]
        public string? Dni { get; set; }

        [MaxLength(50)]
        [Column("direccion")]
        public string? Direccion { get; set; }

        [MaxLength(30)]
        [Column("telefono")]
        public string? Telefono { get; set; }

        [MaxLength(150)]
        [Column("email")]
        public string? Email { get; set; }

        [MaxLength(100)]
        [Column("obra_social")]
        public string? ObraSocial { get; set; }

        [MaxLength(50)]
        [Column("nro_afiliado")]
        public string? NroAfiliado { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("fecha_baja")]
        public DateTime? FechaBaja { get; set; }

        public ICollection<PacienteProfesional> PacienteProfesionales { get; set; } = new List<PacienteProfesional>();
        public ICollection<Informe> Informes { get; set; } = new List<Informe>();
    }
}
