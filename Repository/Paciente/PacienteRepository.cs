using Biblioteca.Entities;
using Biblioteca.Repository;
using Microsoft.EntityFrameworkCore;

namespace Repository.Pacientes
{
    public class PacienteRepository : IPacienteRepository
    {
        private readonly ApplicationDbContext _context;

        public PacienteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Paciente?> GetAsync(Paciente entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            return await _context.Pacientes.FirstOrDefaultAsync(p => p.Id == entity.Id);
        }

        public async Task<IEnumerable<Paciente>> GetAllAsync()
        {
            return await _context.Pacientes.ToListAsync();
        }

        public async Task<IEnumerable<Paciente>> GetAllVinculadosAsync(int profesionalId)
        {
            if (profesionalId <= 0)
                throw new ArgumentOutOfRangeException(nameof(profesionalId), "El id debe ser mayor que 0.");
            return await _context.PacienteProfesionales
                .Where(pp => pp.ProfesionalId == profesionalId)
                .Select(pp => pp.Paciente)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Paciente> Items, int TotalItems)> GetAllPagedAsync(int page, int pageSize, int? profesionalId = null)
        {
            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page), "La página debe ser mayor que 0.");
            if (pageSize < 1 || pageSize > 100)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "El tamaño de página debe estar entre 1 y 100.");
            if (profesionalId.HasValue && profesionalId.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(profesionalId), "El id debe ser mayor que 0.");

            IQueryable<Paciente> query = _context.Pacientes;
            if (profesionalId.HasValue)
            {
                query = _context.PacienteProfesionales
                    .Where(pp => pp.ProfesionalId == profesionalId.Value)
                    .Select(pp => pp.Paciente);
            }

            var skip = (long)(page - 1) * pageSize;
            if (skip > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(page), "La página solicitada es demasiado grande.");

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(p => p.Apellido)
                .ThenBy(p => p.Nombre)
                .ThenBy(p => p.Id)
                .Skip((int)skip)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalItems);
        }

        public async Task<bool> IsVinculadoAsync(int pacienteId, int profesionalId)
        {
            if (pacienteId <= 0)
                throw new ArgumentOutOfRangeException(nameof(pacienteId), "El id debe ser mayor que 0.");
            if (profesionalId <= 0)
                throw new ArgumentOutOfRangeException(nameof(profesionalId), "El id debe ser mayor que 0.");
            return await _context.PacienteProfesionales
                .AnyAsync(pp => pp.PacienteId == pacienteId && pp.ProfesionalId == profesionalId);
        }

        public async Task VincularAsync(int pacienteId, int profesionalId)
        {
            if (pacienteId <= 0)
                throw new ArgumentOutOfRangeException(nameof(pacienteId), "El id debe ser mayor que 0.");
            if (profesionalId <= 0)
                throw new ArgumentOutOfRangeException(nameof(profesionalId), "El id debe ser mayor que 0.");
            var exists = await _context.PacienteProfesionales
                .AnyAsync(pp => pp.ProfesionalId == profesionalId && pp.PacienteId == pacienteId);
            if (!exists)
            {
                _context.PacienteProfesionales.Add(new PacienteProfesional
                {
                    ProfesionalId = profesionalId,
                    PacienteId = pacienteId,
                    FechaVinculacion = DateTime.UtcNow
                });
            }
        }

        public async Task<Paciente> AddAsync(Paciente entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            _context.Pacientes.Add(entity);
            return entity;
        }

        public async Task GuardarCambios()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Paciente> UpdateAsync(Paciente entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            _context.Entry(entity).State = EntityState.Modified;
            return entity;
        }

        public async Task<bool> DeleteAsync(Paciente entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            if (entity.Id <= 0)
                throw new ArgumentOutOfRangeException(nameof(entity.Id), "El id debe ser mayor que 0.");
            var existing = await _context.Pacientes.FindAsync(entity.Id);
            if (existing == null) return false;
            
            _context.Pacientes.Remove(existing);
            return true;
        }

        public async Task<Paciente?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El id debe ser mayor que 0.");
            return await _context.Pacientes.FindAsync(id);
        }

        public async Task<Paciente?> GetByDniAsync(string dni)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dni, nameof(dni));
            return await _context.Pacientes.FirstOrDefaultAsync(p => p.Dni == dni);
        }

        public async Task<Paciente?> GetByEmailAsync(string email)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));
            return await _context.Pacientes.FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<IEnumerable<Paciente>> GetByObraSocialAsync(string obraSocial)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(obraSocial, nameof(obraSocial));
            return await _context.Pacientes
                .Where(p => p.ObraSocial == obraSocial)
                .ToListAsync();
        }
    }
}
