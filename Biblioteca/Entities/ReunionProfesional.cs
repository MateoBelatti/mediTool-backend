using System.ComponentModel.DataAnnotations.Schema;

namespace Biblioteca.Entities
{
    [Table("reunion_profesional")]
    public class ReunionProfesional
    {
        [Column("reunion_id")]
        public int ReunionId { get; set; }
        public Reunion Reunion { get; set; } = null!;

        [Column("profesional_id")]
        public int ProfesionalId { get; set; }
        public Profesional Profesional { get; set; } = null!;
    }
}
