using Utils.DTOs.Informe;

namespace Service.Informes
{
    public interface IInformeService
    {
        Task<IEnumerable<InformeResponseDto>> GetAllAsync();
        Task<InformeResponseDto?> GetByIdAsync(int id);
        Task<IEnumerable<InformeResponseDto>> GetByProfesionalIdAsync(int profesionalId);
        Task<InformeResponseDto?> GetByTurnoIdAsync(int turnoId);
        Task<InformeResponseDto> AddAsync(InformeCreateDto dto);
        Task<InformeResponseDto?> UpdateAsync(int id, InformeUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
