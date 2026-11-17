using Biblioteca.Entities;
using Biblioteca.Repository;
using Microsoft.EntityFrameworkCore;

namespace Repository.Documentos
{
    public class DocumentoPacienteRepository : IDocumentoPacienteRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentoPacienteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DocumentoPaciente>> GetByPacienteAsync(int pacienteId, CancellationToken cancellationToken = default)
        {
            if (pacienteId <= 0)
                throw new ArgumentOutOfRangeException(nameof(pacienteId), "El id debe ser mayor que 0.");

            return await _context.DocumentosPaciente
                .AsNoTracking()
                .Include(d => d.Profesional)
                .Where(d => d.PacienteId == pacienteId)
                .OrderByDescending(d => d.FechaSubida)
                .ToListAsync(cancellationToken);
        }

        public async Task<DocumentoPaciente?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El id debe ser mayor que 0.");

            return await _context.DocumentosPaciente
                .AsNoTracking()
                .Include(d => d.Profesional)
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        }

        public async Task<DocumentoPaciente> AddAsync(DocumentoPaciente entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));

            await _context.ValidarPacienteActivoAsync(entity.PacienteId, cancellationToken);

            if (entity.FechaSubida == default)
                entity.FechaSubida = DateTime.UtcNow;

            await _context.DocumentosPaciente.AddAsync(entity, cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(DocumentoPaciente entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));

            if (entity.Id <= 0)
                throw new ArgumentOutOfRangeException(nameof(entity.Id), "El id debe ser mayor que 0.");

            var existing = await _context.DocumentosPaciente
                .FirstOrDefaultAsync(d => d.Id == entity.Id, cancellationToken);

            if (existing == null) return false;

            _context.DocumentosPaciente.Remove(existing);
            return true;
        }

        public async Task GuardarCambios(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
