using Biblioteca.Entities;
using Biblioteca.Repository;
using Microsoft.EntityFrameworkCore;

namespace Repository.TurnosFijos
{
    public class TurnoFijoRepository : ITurnoFijoRepository
    {
        private readonly ApplicationDbContext _context;

        public TurnoFijoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Actualizar(TurnoFijo turnoFijo)
        {
            ArgumentNullException.ThrowIfNull(turnoFijo, nameof(turnoFijo));
            var persisted = await _context.TurnosFijos
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == turnoFijo.Id);
            if (persisted == null)
                throw new KeyNotFoundException($"TurnoFijo {turnoFijo.Id} no encontrado.");

            if (persisted.PacienteId != turnoFijo.PacienteId || persisted.ProfesionalId != turnoFijo.ProfesionalId)
            {
                await _context.ValidarPacienteActivoAsync(turnoFijo.PacienteId);
                await _context.ValidarVinculacionAsync(turnoFijo.PacienteId, turnoFijo.ProfesionalId);
            }

            _context.TurnosFijos.Update(turnoFijo);
            await Task.CompletedTask;
        }

        public async Task Agregar(TurnoFijo turnoFijo)
        {
            ArgumentNullException.ThrowIfNull(turnoFijo, nameof(turnoFijo));
            await _context.ValidarPacienteActivoAsync(turnoFijo.PacienteId);
            await _context.ValidarVinculacionAsync(turnoFijo.PacienteId, turnoFijo.ProfesionalId);
            await _context.TurnosFijos.AddAsync(turnoFijo);
        }

        public async Task GuardarCambios()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<TurnoFijo>> ObtenerActivos()
        {
            return await _context.TurnosFijos
                .Where(t => t.Activo && t.Paciente.Activo &&
                    _context.PacienteProfesionales.Any(pp => pp.PacienteId == t.PacienteId && pp.ProfesionalId == t.ProfesionalId))
                .ToListAsync();
        }

        public async Task<TurnoFijo?> ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El id debe ser mayor que 0.");
            return await _context.TurnosFijos
                .Include(t => t.Paciente)
                .Include(t => t.Profesional)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<TurnoFijo>> ObtenerPorProfesional(int profesionalId)
        {
            if (profesionalId <= 0)
                throw new ArgumentOutOfRangeException(nameof(profesionalId), "El id debe ser mayor que 0.");
            return await _context.TurnosFijos
                .Include(t => t.Paciente)
                .Include(t => t.Profesional)
                .Where(t => t.ProfesionalId == profesionalId)
                .ToListAsync();
        }
    }
}
