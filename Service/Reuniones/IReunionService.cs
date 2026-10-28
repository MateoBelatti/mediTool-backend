using Utils.DTOs.Reunion;

namespace Service.Reuniones
{
    public interface IReunionService
    {
        Task<IEnumerable<ReunionResponseDto>> GetAllAsync();
        Task<ReunionResponseDto?> GetByIdAsync(int id);
        Task<IEnumerable<ReunionResponseDto>> GetByProfesionalIdAsync(int profesionalId);
        Task<ReunionResponseDto> AddAsync(ReunionCreateDto dto);
        Task<ReunionResponseDto?> UpdateAsync(int id, ReunionUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
