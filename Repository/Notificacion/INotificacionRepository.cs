using Biblioteca.Entities;
using Utils.Enums;

namespace Repository.Notificaciones
{
    public interface INotificacionRepository
    {
        Task<Notificacion> Crear(Notificacion notificacion);
        Task<(IEnumerable<Notificacion> Items, int TotalItems)> ObtenerPorProfesional(int profesionalId, int page, int pageSize, bool? soloNoLeidas = null);
        Task<bool> MarcarComoLeida(int notificacionId, int profesionalId);
        Task<int> MarcarTodasLeidas(int profesionalId);
        Task<int> ContarNoLeidas(int profesionalId);
        Task<bool> ExisteNotificacionHoy(int profesionalId, TipoNotificacion tipo);
        Task GuardarCambios();
    }
}
