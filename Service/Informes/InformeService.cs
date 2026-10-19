using AutoMapper;
using Biblioteca.Entities;
using Biblioteca.Repository;
using Repository.Informes;
using Repository.Pacientes;
using Repository.Turnos;
using Utils.DTOs.Informe;
using Utils.Exceptions;

namespace Service.Informes
{
    public class InformeService : IInformeService
    {
        private readonly IInformeRepository _informeRepository;
        private readonly IPacienteRepository _pacienteRepository;
        private readonly ITurnoRepository _turnoRepository;
        private readonly IMapper _mapper;

        public InformeService(
            IInformeRepository informeRepository,
            IPacienteRepository pacienteRepository,
            ITurnoRepository turnoRepository,
            IMapper mapper)
        {
            _informeRepository = informeRepository;
            _pacienteRepository = pacienteRepository;
            _turnoRepository = turnoRepository;
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

        public async Task<InformeResponseDto?> GetByTurnoIdAsync(int turnoId)
        {
            if (turnoId <= 0)
                throw new ArgumentOutOfRangeException(nameof(turnoId), "El id debe ser mayor que 0.");

            var informe = await _informeRepository.GetByTurnoIdAsync(turnoId);
            if (informe == null) return null;
            return _mapper.Map<InformeResponseDto>(informe);
        }

        public async Task<InformeResponseDto> AddAsync(InformeCreateDto dto)
        {
            if (dto.TurnoId.HasValue)
            {
                var turno = await _turnoRepository.ObtenerPorId(dto.TurnoId.Value);
                if (turno == null)
                    throw new NotFoundError($"Turno {dto.TurnoId.Value} no encontrado.");

                if (turno.ProfesionalId != dto.ProfesionalId)
                    throw new ForbiddenError("El informe solo puede ser creado por el profesional dueño del turno.");

                if (dto.PacienteId.HasValue && dto.PacienteId.Value != turno.PacienteId)
                    throw new ConflictError("El paciente no coincide con el del turno asignado.");

                dto.PacienteId ??= turno.PacienteId;

                if (await _informeRepository.ExisteParaTurnoAsync(dto.TurnoId.Value))
                    throw new ConflictError("El turno seleccionado ya tiene un informe asignado.");
            }

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

            if (dto.TurnoId.HasValue && dto.TurnoId != existing.TurnoId)
            {
                var turno = await _turnoRepository.ObtenerPorId(dto.TurnoId.Value);
                if (turno == null)
                    throw new NotFoundError($"Turno {dto.TurnoId.Value} no encontrado.");

                if (turno.ProfesionalId != dto.ProfesionalId)
                    throw new ForbiddenError("El informe solo puede ser asignado a un turno del mismo profesional.");

                if (dto.PacienteId.HasValue && dto.PacienteId.Value != turno.PacienteId)
                    throw new ConflictError("El paciente no coincide con el del turno asignado.");

                dto.PacienteId ??= turno.PacienteId;

                if (await _informeRepository.ExisteParaTurnoAsync(dto.TurnoId.Value, id))
                    throw new ConflictError("El turno seleccionado ya tiene un informe asignado.");
            }

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
