using Biblioteca.Entities;
using Biblioteca.Repository;
using Microsoft.EntityFrameworkCore;

namespace Repository.Notificaciones
{
    public class NotificacionRepository : INotificacionRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificacionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Notificacion> Crear(Notificacion notificacion)
        {
            ArgumentNullException.ThrowIfNull(notificacion, nameof(notificacion));
            if (notificacion.ProfesionalId <= 0)
                throw new ArgumentOutOfRangeException(nameof(notificacion), "El ProfesionalId debe ser mayor que 0.");
            if (string.IsNullOrWhiteSpace(notificacion.Titulo))
                throw new ArgumentException("El título es obligatorio.", nameof(notificacion));
            if (string.IsNullOrWhiteSpace(notificacion.Mensaje))
                throw new ArgumentException("El mensaje es obligatorio.", nameof(notificacion));

            notificacion.Leida = false;
            if (notificacion.FechaCreacion == default)
                notificacion.FechaCreacion = DateTime.UtcNow;

            await _context.Notificaciones.AddAsync(notificacion);
            return notificacion;
        }

        public async Task<(IEnumerable<Notificacion> Items, int TotalItems)> ObtenerPorProfesional(int profesionalId, int page, int pageSize, bool? soloNoLeidas = null)
        {
            if (profesionalId <= 0)
                throw new ArgumentOutOfRangeException(nameof(profesionalId), "El id debe ser mayor que 0.");
            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page), "La página debe ser mayor que 0.");
            if (pageSize < 1 || pageSize > 1000)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "El tamaño de página debe estar entre 1 y 1000.");

            var query = _context.Notificaciones
                .Where(n => n.ProfesionalId == profesionalId);

            if (soloNoLeidas == true)
            {
                query = query.Where(n => !n.Leida);
            }
            else if (soloNoLeidas == false)
            {
                query = query.Where(n => n.Leida);
            }

            var skip = (long)(page - 1) * pageSize;
            if (skip > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(page), "La página solicitada es demasiado grande.");

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(n => n.FechaCreacion)
                .ThenByDescending(n => n.Id)
                .Skip((int)skip)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalItems);
        }

        public async Task<bool> MarcarComoLeida(int notificacionId, int profesionalId)
        {
            if (notificacionId <= 0)
                throw new ArgumentOutOfRangeException(nameof(notificacionId), "El id debe ser mayor que 0.");
            if (profesionalId <= 0)
                throw new ArgumentOutOfRangeException(nameof(profesionalId), "El id debe ser mayor que 0.");

            var notificacion = await _context.Notificaciones
                .FirstOrDefaultAsync(n =>
                    n.Id == notificacionId &&
                    n.ProfesionalId == profesionalId &&
                    !n.Leida);

            if (notificacion == null)
                return false;

            notificacion.Leida = true;
            return true;
        }

        public async Task<int> MarcarTodasLeidas(int profesionalId)
        {
            if (profesionalId <= 0)
                throw new ArgumentOutOfRangeException(nameof(profesionalId), "El id debe ser mayor que 0.");

            // ExecuteUpdateAsync aplica el cambio directamente en la BD (no requiere GuardarCambios)
            return await _context.Notificaciones
                .Where(n => n.ProfesionalId == profesionalId && !n.Leida)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.Leida, true));
        }

        public async Task<int> ContarNoLeidas(int profesionalId)
        {
            if (profesionalId <= 0)
                throw new ArgumentOutOfRangeException(nameof(profesionalId), "El id debe ser mayor que 0.");

            return await _context.Notificaciones
                .CountAsync(n => n.ProfesionalId == profesionalId && !n.Leida);
        }

        public async Task GuardarCambios()
        {
            await _context.SaveChangesAsync();
        }
    }
}
