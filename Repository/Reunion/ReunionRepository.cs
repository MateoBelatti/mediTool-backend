using Biblioteca.Entities;
using Biblioteca.Repository;
using Microsoft.EntityFrameworkCore;

namespace Repository.Reuniones
{
    public class ReunionRepository : IReunionRepository
    {
        private readonly ApplicationDbContext _context;

        public ReunionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Reunion>> GetAllAsync()
        {
            return await _context.Reuniones
                .AsNoTracking()
                .Include(r => r.Profesional)
                .Include(r => r.Profesionales)
                    .ThenInclude(rp => rp.Profesional)
                .OrderBy(r => r.FechaHora)
                .ToListAsync();
        }

        public async Task<Reunion?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El id debe ser mayor que 0.");

            return await _context.Reuniones
                .Include(r => r.Profesional)
                .Include(r => r.Profesionales)
                    .ThenInclude(rp => rp.Profesional)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Reunion>> GetByProfesionalIdAsync(int profesionalId)
        {
            if (profesionalId <= 0)
                throw new ArgumentOutOfRangeException(nameof(profesionalId), "El id debe ser mayor que 0.");

            var reunionIdsAsignadas = _context.ReunionProfesionales
                .Where(rp => rp.ProfesionalId == profesionalId)
                .Select(rp => rp.ReunionId);

            return await _context.Reuniones
                .AsNoTracking()
                .Include(r => r.Profesional)
                .Include(r => r.Profesionales)
                    .ThenInclude(rp => rp.Profesional)
                .Where(r => r.ProfesionalId == profesionalId
                    || reunionIdsAsignadas.Contains(r.Id))
                .OrderBy(r => r.FechaHora)
                .ToListAsync();
        }

        public async Task<Reunion> AddAsync(Reunion entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            entity.CreatedAt = DateTime.UtcNow;
            await _context.Reuniones.AddAsync(entity);
            return entity;
        }

        public async Task GuardarCambios()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Reunion> UpdateAsync(Reunion entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            var existe = await _context.Reuniones.AnyAsync(r => r.Id == entity.Id);
            if (!existe)
                throw new KeyNotFoundException($"Reunion {entity.Id} no encontrado.");

            _context.Entry(entity).State = EntityState.Modified;
            return entity;
        }

        public async Task<bool> DeleteAsync(Reunion entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            if (entity.Id <= 0)
                throw new ArgumentOutOfRangeException(nameof(entity.Id), "El id debe ser mayor que 0.");

            var existing = await _context.Reuniones
                .Include(r => r.Profesionales)
                .FirstOrDefaultAsync(r => r.Id == entity.Id);

            if (existing == null) return false;

            _context.ReunionProfesionales.RemoveRange(existing.Profesionales);
            _context.Reuniones.Remove(existing);
            return true;
        }
    }
}
