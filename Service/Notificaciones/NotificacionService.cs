using AutoMapper;
using Biblioteca.Entities;
using Repository.Notificaciones;
using Repository.Profesionales;
using Utils.DTOs.Comun;
using Utils.DTOs.Notificacion;
using Utils.Exceptions;

namespace Service.Notificaciones
{
    public class NotificacionService : INotificacionService
    {
        private readonly INotificacionRepository _notificacionRepository;
        private readonly IProfesionalRepository _profesionalRepository;
        private readonly IMapper _mapper;

        public NotificacionService(
            INotificacionRepository notificacionRepository,
            IProfesionalRepository profesionalRepository,
            IMapper mapper)
        {
            _notificacionRepository = notificacionRepository;
            _profesionalRepository = profesionalRepository;
            _mapper = mapper;
        }

        public async Task<PageResult<NotificacionDto>> ObtenerPorProfesionalAsync(int profesionalId, int page, int pageSize, bool? soloNoLeidas = null)
        {
            var result = await _notificacionRepository.ObtenerPorProfesional(profesionalId, page, pageSize, soloNoLeidas);
            var items = _mapper.Map<IEnumerable<NotificacionDto>>(result.Items).ToList();
            var totalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize);

            return new PageResult<NotificacionDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = result.TotalItems,
                TotalPages = totalPages
            };
        }

        public async Task<int> ContarNoLeidasAsync(int profesionalId)
        {
            return await _notificacionRepository.ContarNoLeidas(profesionalId);
        }

        public async Task<NotificacionDto> CrearAsync(CrearNotificacionDto dto)
        {
            var profesional = await _profesionalRepository.GetByIdAsync(dto.ProfesionalId);
            if (profesional == null)
                throw new NotFoundError("Profesional no encontrado.");

            var notificacion = _mapper.Map<Notificacion>(dto);
            var created = await _notificacionRepository.Crear(notificacion);
            await _notificacionRepository.GuardarCambios();
            return _mapper.Map<NotificacionDto>(created);
        }

        public async Task<bool> MarcarComoLeidaAsync(int notificacionId, int profesionalId)
        {
            var marcada = await _notificacionRepository.MarcarComoLeida(notificacionId, profesionalId);
            if (!marcada)
                throw new NotFoundError("Notificación no encontrada.");

            await _notificacionRepository.GuardarCambios();
            return true;
        }

        public async Task<int> MarcarTodasLeidasAsync(int profesionalId)
        {
            return await _notificacionRepository.MarcarTodasLeidas(profesionalId);
        }
    }
}
