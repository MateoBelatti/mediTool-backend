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

            if (persisted.PacienteId != turnoFijo.PacienteId)
            {
                var pacienteActivo = await _context.Pacientes
                    .AnyAsync(p => p.Id == turnoFijo.PacienteId && p.Activo);
                if (!pacienteActivo)
                    throw new KeyNotFoundException($"Paciente {turnoFijo.PacienteId} no encontrado o está dado de baja.");
            }

            _context.TurnosFijos.Update(turnoFijo);
            await Task.CompletedTask;
        }

        public async Task Agregar(TurnoFijo turnoFijo)
        {
            ArgumentNullException.ThrowIfNull(turnoFijo, nameof(turnoFijo));
            var pacienteActivo = await _context.Pacientes
                .AnyAsync(p => p.Id == turnoFijo.PacienteId && p.Activo);
            if (!pacienteActivo)
                throw new KeyNotFoundException($"Paciente {turnoFijo.PacienteId} no encontrado o está dado de baja.");
            await _context.TurnosFijos.AddAsync(turnoFijo);
        }

        public async Task GuardarCambios()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<TurnoFijo>> ObtenerActivos()
        {
            return await _context.TurnosFijos
                .Where(t => t.Activo && t.Paciente.Activo)
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
