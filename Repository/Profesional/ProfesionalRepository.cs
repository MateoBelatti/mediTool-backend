using Biblioteca.Entities;
using Biblioteca.Repository;
using Microsoft.EntityFrameworkCore;

namespace Repository.Profesionales
{
    public class ProfesionalRepository : IProfesionalRepository
    {
        private readonly ApplicationDbContext _context;

        public ProfesionalRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Profesional?> GetAsync(Profesional entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            return await _context.Profesionales.FirstOrDefaultAsync(p => p.Id == entity.Id);
        }

        public async Task<Profesional> AddAsync(Profesional entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            _context.Profesionales.Add(entity);
            return entity;
        }

        public async Task GuardarCambios()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Profesional> UpdateAsync(Profesional entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            _context.Entry(entity).State = EntityState.Modified;
            return entity;
        }

        public async Task<bool> DeleteAsync(Profesional entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            if (entity.Id <= 0)
                throw new ArgumentOutOfRangeException(nameof(entity.Id), "El id debe ser mayor que 0.");
            var existing = await _context.Profesionales.FindAsync(entity.Id);
            if (existing == null) return false;
            
            _context.Profesionales.Remove(existing);
            return true;
        }

        public async Task<Profesional?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El id debe ser mayor que 0.");
            return await _context.Profesionales.FindAsync(id);
        }

        public async Task<Profesional?> GetByEmailAsync(string email)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));
            return await _context.Profesionales.FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<Profesional?> GetByMatriculaAsync(string matricula)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(matricula, nameof(matricula));
            return await _context.Profesionales.FirstOrDefaultAsync(p => p.Matricula == matricula);
        }

        public async Task<Profesional?> GetByRefreshTokenAsync(string refreshToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken, nameof(refreshToken));
            return await _context.Profesionales.FirstOrDefaultAsync(p => p.RefreshToken == refreshToken);
        }

        public async Task VincularPacienteAsync(int profesionalId, int pacienteId)
        {
            if (profesionalId <= 0)
                throw new ArgumentOutOfRangeException(nameof(profesionalId), "El id debe ser mayor que 0.");
            if (pacienteId <= 0)
                throw new ArgumentOutOfRangeException(nameof(pacienteId), "El id debe ser mayor que 0.");

            var pacienteActivo = await _context.Pacientes
                .AnyAsync(p => p.Id == pacienteId && p.Activo);
            if (!pacienteActivo)
                throw new KeyNotFoundException($"Paciente {pacienteId} no encontrado o está dado de baja.");

            var vinculacion = new PacienteProfesional
            {
                ProfesionalId = profesionalId,
                PacienteId = pacienteId,
                FechaVinculacion = DateTime.UtcNow
            };

            // Verificamos si ya existe la vinculación para no duplicar
            var exists = await _context.PacienteProfesionales
                .AnyAsync(pp => pp.ProfesionalId == profesionalId && pp.PacienteId == pacienteId);

            if (!exists)
            {
                _context.PacienteProfesionales.Add(vinculacion);
            }
        }

        public async Task<IEnumerable<Paciente>> GetPacientesVinculadosAsync(int profesionalId)
        {
            if (profesionalId <= 0)
                throw new ArgumentOutOfRangeException(nameof(profesionalId), "El id debe ser mayor que 0.");
            return await _context.PacienteProfesionales
                .Where(pp => pp.ProfesionalId == profesionalId && pp.Paciente.Activo)
                .Select(pp => pp.Paciente)
                .ToListAsync();
        }
    }
}
