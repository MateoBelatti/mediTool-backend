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
            return await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Id == entity.Id && p.Activo);
        }

        public async Task<IEnumerable<Paciente>> GetAllAsync()
        {
            return await _context.Pacientes
                .Where(p => p.Activo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Paciente>> GetAllVinculadosAsync(int profesionalId)
        {
            if (profesionalId <= 0)
                throw new ArgumentOutOfRangeException(nameof(profesionalId), "El id debe ser mayor que 0.");
            return await _context.PacienteProfesionales
                .Where(pp => pp.ProfesionalId == profesionalId && pp.Paciente.Activo)
                .Select(pp => pp.Paciente)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Paciente> Items, int TotalItems)> GetAllPagedAsync(int page, int pageSize, int? profesionalId = null, string? searchTerm = null, string? sortBy = "nombre", string? sortOrder = "asc")
        {
            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page), "La página debe ser mayor que 0.");
            if (pageSize < 1 || pageSize > 100)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "El tamaño de página debe estar entre 1 y 100.");
            if (profesionalId.HasValue && profesionalId.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(profesionalId), "El id debe ser mayor que 0.");

            IQueryable<Paciente> query = _context.Pacientes.Where(p => p.Activo);
            if (profesionalId.HasValue)
            {
                query = _context.PacienteProfesionales
                    .Where(pp => pp.ProfesionalId == profesionalId.Value && pp.Paciente.Activo)
                    .Select(pp => pp.Paciente);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(p => 
                    p.Nombre.ToLower().Contains(term) || 
                    p.Apellido.ToLower().Contains(term) || 
                    (p.Dni != null && p.Dni.Contains(term)) || 
                    (p.ObraSocial != null && p.ObraSocial.ToLower().Contains(term)));
            }

            var skip = (long)(page - 1) * pageSize;
            if (skip > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(page), "La página solicitada es demasiado grande.");

            var totalItems = await query.CountAsync();

            bool isDesc = sortOrder?.ToLower() == "desc";
            query = sortBy?.ToLower() switch
            {
                "apellido" => isDesc ? query.OrderByDescending(p => p.Apellido).ThenByDescending(p => p.Nombre).ThenBy(p => p.Id) : query.OrderBy(p => p.Apellido).ThenBy(p => p.Nombre).ThenBy(p => p.Id),
                "dni" => isDesc ? query.OrderByDescending(p => p.Dni).ThenBy(p => p.Id) : query.OrderBy(p => p.Dni).ThenBy(p => p.Id),
                "obrasocial" => isDesc ? query.OrderByDescending(p => p.ObraSocial).ThenBy(p => p.Id) : query.OrderBy(p => p.ObraSocial).ThenBy(p => p.Id),
                _ => isDesc ? query.OrderByDescending(p => p.Nombre).ThenByDescending(p => p.Apellido).ThenBy(p => p.Id) : query.OrderBy(p => p.Nombre).ThenBy(p => p.Apellido).ThenBy(p => p.Id)
            };

            var items = await query
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
            return await _context.EsVinculadoAsync(pacienteId, profesionalId);
        }

        public async Task VincularAsync(int pacienteId, int profesionalId)
        {
            await _context.VincularAsync(pacienteId, profesionalId);
        }

        public async Task<IEnumerable<Profesional>> GetProfesionalesVinculadosAsync(int pacienteId)
        {
            if (pacienteId <= 0)
                throw new ArgumentOutOfRangeException(nameof(pacienteId), "El id debe ser mayor que 0.");

            return await _context.PacienteProfesionales
                .Where(pp => pp.PacienteId == pacienteId)
                .Select(pp => pp.Profesional)
                .ToListAsync();
        }

        public async Task<Paciente> AddAsync(Paciente entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            entity.Activo = true;
            entity.FechaBaja = null;
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

            var existing = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Id == entity.Id);
            if (existing == null || !existing.Activo) return false;

            existing.Activo = false;
            existing.FechaBaja = DateTime.UtcNow;
            return true;
        }

        public async Task<Paciente?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El id debe ser mayor que 0.");
            return await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Id == id && p.Activo);
        }

        public async Task<Paciente?> GetByIdIncludingInactiveAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El id debe ser mayor que 0.");
            return await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Paciente?> GetByDniAsync(string dni)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dni, nameof(dni));
            return await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Dni == dni && p.Activo);
        }

        public async Task<Paciente?> GetByDniIncludingInactiveAsync(string dni)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dni, nameof(dni));
            return await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Dni == dni);
        }

        public async Task<Paciente?> GetByEmailAsync(string email)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));
            return await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Email == email && p.Activo);
        }

        public async Task<Paciente?> GetByEmailIncludingInactiveAsync(string email)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));
            return await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<IEnumerable<Paciente>> GetByObraSocialAsync(string obraSocial)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(obraSocial, nameof(obraSocial));
            return await _context.Pacientes
                .Where(p => p.ObraSocial == obraSocial && p.Activo)
                .ToListAsync();
        }
    }
}
