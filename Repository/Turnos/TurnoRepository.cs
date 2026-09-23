using Biblioteca.Entities;
using Biblioteca.Repository;
using Microsoft.EntityFrameworkCore;

namespace Repository.Turnos
{
    public class TurnoRepository : ITurnoRepository
    {
        private readonly ApplicationDbContext _context;

        public TurnoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        private DateTime ToUtc(DateTime date)
        {
            return date.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(date, DateTimeKind.Utc) : date.ToUniversalTime();
        }

        public async Task Actualizar(Turno turno)
        {
            ArgumentNullException.ThrowIfNull(turno, nameof(turno));
            var existe = await _context.Turnos.AnyAsync(t => t.Id == turno.Id);
            if (!existe)
                throw new KeyNotFoundException($"Turno {turno.Id} no encontrado.");
            _context.Turnos.Update(turno);
            await Task.CompletedTask;
        }

        public async Task ActualizarRango(List<Turno> turnos)
        {
            ArgumentNullException.ThrowIfNull(turnos, nameof(turnos));
            _context.Turnos.UpdateRange(turnos);
            await Task.CompletedTask;
        }

        public async Task Agregar(Turno turno)
        {
            ArgumentNullException.ThrowIfNull(turno, nameof(turno));
            await _context.Turnos.AddAsync(turno);
        }

        public async Task<bool> ExisteSolapamiento(int profesionalId, DateTime fechaHora, int duracionMin, int? excluirTurnoId = null)
        {
            if (profesionalId <= 0)
                throw new ArgumentOutOfRangeException(nameof(profesionalId), "El id debe ser mayor que 0.");
            fechaHora = ToUtc(fechaHora);
            var finNuevo = fechaHora.AddMinutes(duracionMin);
            return await _context.Turnos.AnyAsync(t => 
                t.ProfesionalId == profesionalId &&
                (!excluirTurnoId.HasValue || t.Id != excluirTurnoId.Value) &&
                t.Estado != "Cancelado" &&
                t.FechaHora < finNuevo && 
                t.FechaHora.AddMinutes(t.DuracionMin) > fechaHora);
        }

        public async Task GuardarCambios()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<Turno>> ObtenerFuturosPorTurnoFijo(int turnoFijoId, DateTime desde)
        {
            if (turnoFijoId <= 0)
                throw new ArgumentOutOfRangeException(nameof(turnoFijoId), "El id debe ser mayor que 0.");
            desde = ToUtc(desde);
            return await _context.Turnos
                .Where(t => t.TurnoFijoId == turnoFijoId && t.FechaHora >= desde)
                .OrderBy(t => t.FechaHora)
                .ToListAsync();
        }

        public async Task<Turno?> ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El id debe ser mayor que 0.");
            return await _context.Turnos
                .Include(t => t.Paciente)
                .Include(t => t.Profesional)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Turno>> ObtenerPorRangoFecha(DateTime desde, DateTime hasta, int? profesionalId = null)
        {
            desde = ToUtc(desde);
            hasta = ToUtc(hasta);
            var query = _context.Turnos
                .Include(t => t.Paciente)
                .Include(t => t.Profesional)
                .Where(t => t.FechaHora >= desde && t.FechaHora <= hasta);
            
            if (profesionalId.HasValue)
            {
                query = query.Where(t => t.ProfesionalId == profesionalId.Value);
            }

            return await query.OrderBy(t => t.FechaHora).ToListAsync();
        }

        public async Task<List<Turno>> ObtenerFacturables(int pacienteId, DateTime desde, DateTime hasta)
        {
            if (pacienteId <= 0)
                throw new ArgumentOutOfRangeException(nameof(pacienteId), "El id debe ser mayor que 0.");
            desde = ToUtc(desde);
            hasta = ToUtc(hasta);
            return await _context.Turnos
                .Include(t => t.Paciente)
                .Include(t => t.Profesional)
                .Where(t => t.PacienteId == pacienteId &&
                            t.Facturable &&
                            t.Estado != "Cancelado" &&
                            t.FechaHora >= desde &&
                            t.FechaHora <= hasta)
                .OrderBy(t => t.FechaHora)
                .ToListAsync();
        }

        public async Task<Turno?> ObtenerUltimoPorTurnoFijo(int turnoFijoId)
        {
            if (turnoFijoId <= 0)
                throw new ArgumentOutOfRangeException(nameof(turnoFijoId), "El id debe ser mayor que 0.");
            return await _context.Turnos
                .Where(t => t.TurnoFijoId == turnoFijoId)
                .OrderByDescending(t => t.FechaHora)
                .FirstOrDefaultAsync();
        }
    }
}
