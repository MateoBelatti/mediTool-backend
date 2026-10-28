using AutoMapper;
using Biblioteca.Entities;
using Repository.Reuniones;
using Utils.DTOs.Reunion;
using Utils.Helpers;

namespace Service.Reuniones
{
    public class ReunionService : IReunionService
    {
        private readonly IReunionRepository _reunionRepository;
        private readonly IMapper _mapper;

        public ReunionService(IReunionRepository reunionRepository, IMapper mapper)
        {
            _reunionRepository = reunionRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReunionResponseDto>> GetAllAsync()
        {
            var reuniones = await _reunionRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ReunionResponseDto>>(reuniones);
        }

        public async Task<ReunionResponseDto?> GetByIdAsync(int id)
        {
            var reunion = await _reunionRepository.GetByIdAsync(id);
            if (reunion == null) return null;
            return _mapper.Map<ReunionResponseDto>(reunion);
        }

        public async Task<IEnumerable<ReunionResponseDto>> GetByProfesionalIdAsync(int profesionalId)
        {
            var reuniones = await _reunionRepository.GetByProfesionalIdAsync(profesionalId);
            return _mapper.Map<IEnumerable<ReunionResponseDto>>(reuniones);
        }

        public async Task<ReunionResponseDto> AddAsync(ReunionCreateDto dto)
        {
            var reunion = _mapper.Map<Reunion>(dto);
            reunion.FechaHora = DateTimeHelper.NormalizarAUtc(reunion.FechaHora);
            var created = await _reunionRepository.AddAsync(reunion);
            await _reunionRepository.GuardarCambios();
            return _mapper.Map<ReunionResponseDto>(created);
        }

        public async Task<ReunionResponseDto?> UpdateAsync(int id, ReunionUpdateDto dto)
        {
            var existing = await _reunionRepository.GetByIdAsync(id);
            if (existing == null) return null;

            _mapper.Map(dto, existing);
            existing.FechaHora = DateTimeHelper.NormalizarAUtc(existing.FechaHora);
            var updated = await _reunionRepository.UpdateAsync(existing);
            await _reunionRepository.GuardarCambios();
            return _mapper.Map<ReunionResponseDto>(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _reunionRepository.GetByIdAsync(id);
            if (existing == null) return false;

            var success = await _reunionRepository.DeleteAsync(existing);
            await _reunionRepository.GuardarCambios();
            return success;
        }
    }
}
