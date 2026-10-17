using Utils.Enums;

namespace Utils.DTOs.Notificacion
{
    public class NotificacionDto
    {
        public int Id { get; set; }
        public int ProfesionalId { get; set; }
        public TipoNotificacion Tipo { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public bool Leida { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
