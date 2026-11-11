using Biblioteca.Entities;
using Biblioteca.Repository;
using Microsoft.EntityFrameworkCore;

namespace Repository.Informes
{
    public class InformeRepository : IInformeRepository
    {
        private readonly ApplicationDbContext _context;

        public InformeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Informe>> GetAllAsync()
        {
            return await _context.Informes
                .Include(i => i.Turno)
                .ToListAsync();
        }

        public async Task<Informe?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El id debe ser mayor que 0.");
            return await _context.Informes
                .Include(i => i.Turno)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<Informe>> GetByProfesionalIdAsync(int profesionalId)
        {
            if (profesionalId <= 0)
                throw new ArgumentOutOfRangeException(nameof(profesionalId), "El id debe ser mayor que 0.");
            return await _context.Informes
                .Include(i => i.Turno)
                .Where(i => i.ProfesionalId == profesionalId)
                .ToListAsync();
        }

        public async Task<Informe?> GetByTurnoIdAsync(int turnoId)
        {
            if (turnoId <= 0)
                throw new ArgumentOutOfRangeException(nameof(turnoId), "El id debe ser mayor que 0.");
            return await _context.Informes
                .Include(i => i.Turno)
                .FirstOrDefaultAsync(i => i.Turno != null && i.Turno.Id == turnoId);
        }

        public async Task<Informe> AddAsync(Informe entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            await _context.ValidarPacienteActivoAsync(entity.PacienteId);
            await _context.ValidarVinculacionAsync(entity.PacienteId, entity.ProfesionalId);
            entity.CreatedAt = DateTime.UtcNow;
            await _context.Informes.AddAsync(entity);
            return entity;
        }

        public async Task GuardarCambios()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Informe> UpdateAsync(Informe entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            var persisted = await _context.Informes
                .FirstOrDefaultAsync(i => i.Id == entity.Id);
            if (persisted == null)
                throw new KeyNotFoundException($"Informe {entity.Id} no encontrado.");

            var entry = _context.Entry(persisted);
            var pacienteOriginalId = entry.OriginalValues.GetValue<int?>(nameof(Informe.PacienteId));
            var profesionalOriginalId = entry.OriginalValues.GetValue<int>(nameof(Informe.ProfesionalId));

            if (pacienteOriginalId != entity.PacienteId || profesionalOriginalId != entity.ProfesionalId)
            {
                await _context.ValidarPacienteActivoAsync(entity.PacienteId);
                await _context.ValidarVinculacionAsync(entity.PacienteId, entity.ProfesionalId);
            }

            entry.CurrentValues.SetValues(entity);
            return persisted;
        }

        public async Task<bool> DeleteAsync(Informe entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            if (entity.Id <= 0)
                throw new ArgumentOutOfRangeException(nameof(entity.Id), "El id debe ser mayor que 0.");
            var existing = await _context.Informes.FindAsync(entity.Id);
            if (existing == null) return false;
            _context.Informes.Remove(existing);
            return true;
        }
    }
}
