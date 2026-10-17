using Utils.DTOs.Comun;
using Utils.DTOs.Notificacion;

namespace Service.Notificaciones
{
    public interface INotificacionService
    {
        Task<PageResult<NotificacionDto>> ObtenerPorProfesionalAsync(int profesionalId, int page, int pageSize, bool? soloNoLeidas = null);
        Task<int> ContarNoLeidasAsync(int profesionalId);
        Task<NotificacionDto> CrearAsync(CrearNotificacionDto dto);
        Task<bool> MarcarComoLeidaAsync(int notificacionId, int profesionalId);
        Task<int> MarcarTodasLeidasAsync(int profesionalId);
    }
}
