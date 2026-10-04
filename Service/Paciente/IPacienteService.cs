using Utils.DTOs.Comun;
using Utils.DTOs.Paciente;

namespace Service.Pacientes
{
    public interface IPacienteService
    {
        Task<IEnumerable<PacienteResponseDto>> GetAllAsync(int? profesionalId = null);
        Task<PageResult<PacienteResponseDto>> GetAllPagedAsync(int page, int pageSize, int? profesionalId = null, string? searchTerm = null, string? sortBy = "nombre", string? sortOrder = "asc");
        Task<PacienteResponseDto> AddAsync(PacienteCreateDto dto, int? profesionalId = null);
        Task<PacienteResponseDto?> UpdateAsync(int id, PacienteUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<PacienteResponseDto?> GetByIdAsync(int id);
        Task<bool> IsVinculadoAsync(int pacienteId, int profesionalId);
        Task<IEnumerable<Utils.DTOs.Profesional.ProfesionalResponseDto>> GetProfesionalesVinculadosAsync(int pacienteId);
        Task<PacienteResponseDto?> GetByDniAsync(string dni);
        Task<PacienteResponseDto?> GetByEmailAsync(string email);
        Task<IEnumerable<PacienteResponseDto>> GetByObraSocialAsync(string obraSocial);
    }
}
