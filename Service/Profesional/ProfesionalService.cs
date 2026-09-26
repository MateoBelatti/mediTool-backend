using AutoMapper;
using Biblioteca.Entities;
using Repository.Profesionales;
using Utils.DTOs.Profesional;
using Utils.Exceptions;

namespace Service.Profesionales
{
    public class ProfesionalService : IProfesionalService
    {
        private readonly IProfesionalRepository _repository;
        private readonly IMapper _mapper;

        public ProfesionalService(IProfesionalRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ProfesionalResponseDto?> GetAsync(ProfesionalResponseDto dto)
        {
            var entity = _mapper.Map<Profesional>(dto);
            var result = await _repository.GetAsync(entity);
            return _mapper.Map<ProfesionalResponseDto>(result);
        }

        public async Task<ProfesionalResponseDto> AddAsync(ProfesionalCreateDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var existingEmail = await _repository.GetByEmailAsync(dto.Email);
                if (existingEmail != null)
                    throw new ConflictError("Ya existe un profesional con este email.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Matricula))
            {
                var existingMatricula = await _repository.GetByMatriculaAsync(dto.Matricula);
                if (existingMatricula != null)
                    throw new ConflictError("Ya existe un profesional con esta matrícula.");
            }

            var entity = _mapper.Map<Profesional>(dto);
            if (!string.IsNullOrWhiteSpace(entity.Password))
            {
                entity.Password = BCrypt.Net.BCrypt.HashPassword(entity.Password);
            }
            var result = await _repository.AddAsync(entity);
            await _repository.GuardarCambios();
            return _mapper.Map<ProfesionalResponseDto>(result);
        }

        public async Task<ProfesionalResponseDto?> UpdateAsync(int id, ProfesionalUpdateDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return null;

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != existing.Email)
            {
                var existingEmail = await _repository.GetByEmailAsync(dto.Email);
                if (existingEmail != null && existingEmail.Id != id)
                    throw new ConflictError("Ya existe un profesional con este email.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Matricula) && dto.Matricula != existing.Matricula)
            {
                var existingMatricula = await _repository.GetByMatriculaAsync(dto.Matricula);
                if (existingMatricula != null && existingMatricula.Id != id)
                    throw new ConflictError("Ya existe un profesional con esta matrícula.");
            }

            _mapper.Map(dto, existing);
            var result = await _repository.UpdateAsync(existing);
            await _repository.GuardarCambios();
            return _mapper.Map<ProfesionalResponseDto>(result);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            var success = await _repository.DeleteAsync(existing);
            await _repository.GuardarCambios();
            return success;
        }

        public async Task<ProfesionalResponseDto?> GetByIdAsync(int id)
        {
            var result = await _repository.GetByIdAsync(id);
            return _mapper.Map<ProfesionalResponseDto>(result);
        }

        public async Task<ProfesionalResponseDto?> GetByEmailAsync(string email)
        {
            var result = await _repository.GetByEmailAsync(email);
            return _mapper.Map<ProfesionalResponseDto>(result);
        }

        public async Task<ProfesionalResponseDto?> GetByMatriculaAsync(string matricula)
        {
            var result = await _repository.GetByMatriculaAsync(matricula);
            return _mapper.Map<ProfesionalResponseDto>(result);
        }

        public async Task VincularPacienteAsync(int profesionalId, int pacienteId)
        {
            await _repository.VincularPacienteAsync(profesionalId, pacienteId);
            await _repository.GuardarCambios();
        }

        public async Task<bool> DesvincularPacienteAsync(int profesionalId, int pacienteId)
        {
            var desvinculado = await _repository.DesvincularPacienteAsync(profesionalId, pacienteId);
            if (!desvinculado)
                return false;

            await _repository.GuardarCambios();
            return true;
        }
    }
}
