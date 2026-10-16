using Biblioteca.Entities;

namespace Repository.Notificaciones
{
    public interface INotificacionRepository
    {
        Task<Notificacion> Crear(Notificacion notificacion);
        Task<(IEnumerable<Notificacion> Items, int TotalItems)> ObtenerPorProfesional(int profesionalId, int page, int pageSize, bool? soloNoLeidas = null);
        Task<bool> MarcarComoLeida(int notificacionId, int profesionalId);
        Task<int> MarcarTodasLeidas(int profesionalId);
        Task<int> ContarNoLeidas(int profesionalId);
        Task GuardarCambios();
    }
}
