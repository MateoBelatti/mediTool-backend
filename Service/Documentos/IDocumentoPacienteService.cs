using Utils.DTOs.Documento;

namespace Service.Documentos
{
    public interface IDocumentoPacienteService
    {
        Task<IEnumerable<DocumentoPacienteResponseDto>> GetByPacienteAsync(int pacienteId, CancellationToken cancellationToken = default);
        Task<ArchivoDocumento?> GetArchivoAsync(int pacienteId, int documentoId, CancellationToken cancellationToken = default);
        Task<DocumentoPacienteResponseDto> AddAsync(int pacienteId, int profesionalId, DocumentoSubidaDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int pacienteId, int documentoId, CancellationToken cancellationToken = default);
    }

    public record ArchivoDocumento(DocumentoPacienteResponseDto Documento, Stream Contenido);
}
