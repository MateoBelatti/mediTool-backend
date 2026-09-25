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
            return await _context.Informes.ToListAsync();
        }

        public async Task<Informe?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El id debe ser mayor que 0.");
            return await _context.Informes.FindAsync(id);
        }

        public async Task<IEnumerable<Informe>> GetByProfesionalIdAsync(int profesionalId)
        {
            if (profesionalId <= 0)
                throw new ArgumentOutOfRangeException(nameof(profesionalId), "El id debe ser mayor que 0.");
            return await _context.Informes
                .Where(i => i.ProfesionalId == profesionalId)
                .ToListAsync();
        }

        public async Task<Informe> AddAsync(Informe entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            await ValidarPacienteActivoAsync(entity.PacienteId);
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
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == entity.Id);
            if (persisted == null)
                throw new KeyNotFoundException($"Informe {entity.Id} no encontrado.");

            if (persisted.PacienteId != entity.PacienteId)
                await ValidarPacienteActivoAsync(entity.PacienteId);

            _context.Informes.Update(entity);
            return entity;
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

        private async Task ValidarPacienteActivoAsync(int? pacienteId)
        {
            if (!pacienteId.HasValue)
                return;

            var pacienteActivo = await _context.Pacientes
                .AnyAsync(p => p.Id == pacienteId.Value && p.Activo);
            if (!pacienteActivo)
                throw new KeyNotFoundException($"Paciente {pacienteId.Value} no encontrado o está dado de baja.");
        }
    }
}
