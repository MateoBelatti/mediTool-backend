using Biblioteca.Entities;

namespace Repository.Documentos
{
    public interface IDocumentoPacienteRepository
    {
        Task<IEnumerable<DocumentoPaciente>> GetByPacienteAsync(int pacienteId, CancellationToken cancellationToken = default);
        Task<DocumentoPaciente?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<DocumentoPaciente> AddAsync(DocumentoPaciente entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(DocumentoPaciente entity, CancellationToken cancellationToken = default);
        Task GuardarCambios(CancellationToken cancellationToken = default);
    }
}
