using Utils.DTOs.Profesional;

namespace Service.Profesionales
{
    public interface IProfesionalService
    {
        Task<ProfesionalResponseDto?> GetAsync(ProfesionalResponseDto dto);
        Task<ProfesionalResponseDto> AddAsync(ProfesionalCreateDto dto);
        Task<ProfesionalResponseDto?> UpdateAsync(int id, ProfesionalUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<ProfesionalResponseDto?> GetByIdAsync(int id);
        Task<ProfesionalResponseDto?> GetByEmailAsync(string email);
        Task<ProfesionalResponseDto?> GetByMatriculaAsync(string matricula);
        Task VincularPacienteAsync(int profesionalId, int pacienteId);
        Task<bool> DesvincularPacienteAsync(int profesionalId, int pacienteId);
    }
}
