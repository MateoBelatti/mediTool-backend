using AutoMapper;
using Biblioteca.Entities;
using Biblioteca.Repository;
using Repository.Informes;
using Repository.Pacientes;
using Utils.DTOs.Informe;
using Utils.Exceptions;

namespace Service.Informes
{
    public class InformeService : IInformeService
    {
        private readonly IInformeRepository _informeRepository;
        private readonly IPacienteRepository _pacienteRepository;
        private readonly IMapper _mapper;

        public InformeService(IInformeRepository informeRepository, IPacienteRepository pacienteRepository, IMapper mapper)
        {
            _informeRepository = informeRepository;
            _pacienteRepository = pacienteRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<InformeResponseDto>> GetAllAsync()
        {
            var informes = await _informeRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<InformeResponseDto>>(informes);
        }

        public async Task<InformeResponseDto?> GetByIdAsync(int id)
        {
            var informe = await _informeRepository.GetByIdAsync(id);
            if (informe == null) return null;
            return _mapper.Map<InformeResponseDto>(informe);
        }

        public async Task<IEnumerable<InformeResponseDto>> GetByProfesionalIdAsync(int profesionalId)
        {
            var informes = await _informeRepository.GetByProfesionalIdAsync(profesionalId);
            return _mapper.Map<IEnumerable<InformeResponseDto>>(informes);
        }

        public async Task<InformeResponseDto> AddAsync(InformeCreateDto dto)
        {
            var informe = _mapper.Map<Informe>(dto);

            if (informe.PacienteId.HasValue &&
                !await _pacienteRepository.IsVinculadoAsync(informe.PacienteId.Value, informe.ProfesionalId))
            {
                throw new ConflictError(ApplicationDbContextExtensions.MensajeNoVinculado);
            }

            var created = await _informeRepository.AddAsync(informe);
            await _informeRepository.GuardarCambios();
            return _mapper.Map<InformeResponseDto>(created);
        }

        public async Task<InformeResponseDto?> UpdateAsync(int id, InformeUpdateDto dto)
        {
            var existing = await _informeRepository.GetByIdAsync(id);
            if (existing == null) return null;

            if (dto.PacienteId.HasValue &&
                (dto.PacienteId != existing.PacienteId || dto.ProfesionalId != existing.ProfesionalId) &&
                !await _pacienteRepository.IsVinculadoAsync(dto.PacienteId.Value, dto.ProfesionalId))
            {
                throw new ConflictError(ApplicationDbContextExtensions.MensajeNoVinculado);
            }

            _mapper.Map(dto, existing);
            var updated = await _informeRepository.UpdateAsync(existing);
            await _informeRepository.GuardarCambios();
            return _mapper.Map<InformeResponseDto>(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _informeRepository.GetByIdAsync(id);
            if (existing == null) return false;

            var success = await _informeRepository.DeleteAsync(existing);
            await _informeRepository.GuardarCambios();
            return success;
        }
    }
}
