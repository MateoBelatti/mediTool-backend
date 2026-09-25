using AutoMapper;
using Biblioteca.Entities;
using Repository.Pacientes;
using Utils.DTOs.Comun;
using Utils.DTOs.Paciente;
using Utils.Exceptions;
using System.Text.RegularExpressions;

namespace Service.Pacientes
{
    public class PacienteService : IPacienteService
    {
        private readonly IPacienteRepository _repository;
        private readonly IMapper _mapper;
        private static readonly Regex DniRegex = new(@"^\d{7,8}$", RegexOptions.Compiled);
        private static readonly Regex PhoneRegex = new(@"^[\d\s\-\+\(\)]{6,30}$", RegexOptions.Compiled);
        private const int MaxAgeYears = 120;
        private const int MaxPageSize = 100;

        public PacienteService(IPacienteRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PacienteResponseDto>> GetAllAsync(int? profesionalId = null)
        {
            var result = profesionalId.HasValue
                ? await _repository.GetAllVinculadosAsync(profesionalId.Value)
                : await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<PacienteResponseDto>>(result);
        }

        public async Task<PageResult<PacienteResponseDto>> GetAllPagedAsync(int page, int pageSize, int? profesionalId = null)
        {
            ValidatePagination(page, pageSize);

            var result = await _repository.GetAllPagedAsync(page, pageSize, profesionalId);
            var items = _mapper.Map<IEnumerable<PacienteResponseDto>>(result.Items).ToList();
            var totalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize);

            return new PageResult<PacienteResponseDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = result.TotalItems,
                TotalPages = totalPages
            };
        }

        public async Task<PacienteResponseDto> AddAsync(PacienteCreateDto dto, int? profesionalId = null)
        {
            ValidatePacienteDto(dto);

            string? normalizedEmail = NormalizeEmail(dto.Email);
            string? normalizedDni = NormalizeDni(dto.Dni);

            if (!string.IsNullOrWhiteSpace(normalizedDni))
            {
                var existingDni = await _repository.GetByDniAsync(normalizedDni);
                if (existingDni != null)
                    throw new ConflictError("Ya existe un paciente con este DNI.");
            }

            if (!string.IsNullOrWhiteSpace(normalizedEmail))
            {
                var existingEmail = await _repository.GetByEmailAsync(normalizedEmail);
                if (existingEmail != null)
                    throw new ConflictError("Ya existe un paciente con este email.");
            }

            var entity = _mapper.Map<Paciente>(dto);
            entity.Email = normalizedEmail;
            entity.Dni = normalizedDni;
            var result = await _repository.AddAsync(entity);
            await _repository.GuardarCambios();

            if (profesionalId.HasValue)
            {
                await _repository.VincularAsync(result.Id, profesionalId.Value);
                await _repository.GuardarCambios();
            }

            return _mapper.Map<PacienteResponseDto>(result);
        }

        public async Task<PacienteResponseDto?> UpdateAsync(int id, PacienteUpdateDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return null;

            ValidatePacienteDto(dto);

            string? normalizedEmail = NormalizeEmail(dto.Email);
            string? normalizedDni = NormalizeDni(dto.Dni);

            if (!string.IsNullOrWhiteSpace(normalizedDni) && normalizedDni != existing.Dni)
            {
                var existingDni = await _repository.GetByDniAsync(normalizedDni);
                if (existingDni != null && existingDni.Id != id)
                    throw new ConflictError("Ya existe un paciente con este DNI.");
            }

            if (!string.IsNullOrWhiteSpace(normalizedEmail) && normalizedEmail != existing.Email)
            {
                var existingEmail = await _repository.GetByEmailAsync(normalizedEmail);
                if (existingEmail != null && existingEmail.Id != id)
                    throw new ConflictError("Ya existe un paciente con este email.");
            }

            _mapper.Map(dto, existing);
            existing.Email = normalizedEmail;
            existing.Dni = normalizedDni;
            var result = await _repository.UpdateAsync(existing);
            await _repository.GuardarCambios();
            return _mapper.Map<PacienteResponseDto>(result);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            var success = await _repository.DeleteAsync(existing);
            await _repository.GuardarCambios();
            return success;
        }

        public async Task<PacienteResponseDto?> GetByIdAsync(int id)
        {
            var result = await _repository.GetByIdAsync(id);
            return _mapper.Map<PacienteResponseDto>(result);
        }

        public async Task<bool> IsVinculadoAsync(int pacienteId, int profesionalId)
        {
            return await _repository.IsVinculadoAsync(pacienteId, profesionalId);
        }

        public async Task<PacienteResponseDto?> GetByDniAsync(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni))
                throw new ValidationError("El DNI es obligatorio.");
            
            string normalizedDni = NormalizeDni(dni);
            if (!DniRegex.IsMatch(normalizedDni))
                throw new ValidationError("El DNI debe contener solo dígitos (7 u 8 caracteres).");

            var result = await _repository.GetByDniAsync(normalizedDni);
            return _mapper.Map<PacienteResponseDto>(result);
        }

        public async Task<PacienteResponseDto?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ValidationError("El email es obligatorio.");
            
            string normalizedEmail = NormalizeEmail(email);
            var result = await _repository.GetByEmailAsync(normalizedEmail);
            return _mapper.Map<PacienteResponseDto>(result);
        }

        public async Task<IEnumerable<PacienteResponseDto>> GetByObraSocialAsync(string obraSocial)
        {
            if (string.IsNullOrWhiteSpace(obraSocial))
                throw new ValidationError("La obra social es obligatoria.");

            var result = await _repository.GetByObraSocialAsync(obraSocial.Trim());
            return _mapper.Map<IEnumerable<PacienteResponseDto>>(result);
        }

        private static void ValidatePagination(int page, int pageSize)
        {
            if (page < 1)
                throw new ValidationError("La página debe ser mayor o igual que 1.");

            if (pageSize < 1 || pageSize > MaxPageSize)
                throw new ValidationError($"El tamaño de página debe estar entre 1 y {MaxPageSize}.");
        }

        private static void ValidatePacienteDto(dynamic dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.Dni))
            {
                string normalizedDni = NormalizeDni(dto.Dni);
                if (!DniRegex.IsMatch(normalizedDni))
                    throw new ValidationError("El DNI debe contener solo dígitos (7 u 8 caracteres).");
            }

            if (dto.FechaNacimiento.HasValue)
            {
                var fechaNac = dto.FechaNacimiento.Value;
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                
                if (fechaNac > today)
                    throw new ValidationError("La fecha de nacimiento no puede ser futura.");
                
                var age = today.Year - fechaNac.Year;
                if (fechaNac > today.AddYears(-age)) age--;
                if (age > MaxAgeYears)
                    throw new ValidationError($"La fecha de nacimiento no puede ser anterior a {MaxAgeYears} años.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Telefono))
            {
                if (!PhoneRegex.IsMatch(dto.Telefono))
                    throw new ValidationError("El formato del teléfono no es válido.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                try
                {
                    var addr = new System.Net.Mail.MailAddress(dto.Email);
                    if (addr.Address != dto.Email.Trim())
                        throw new ValidationError("El email contiene espacios innecesarios.");
                }
                catch (FormatException)
                {
                    throw new ValidationError("El formato del correo electrónico no es válido.");
                }
            }
        }

        private static string? NormalizeEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            return email.Trim().ToLowerInvariant();
        }

        private static string? NormalizeDni(string? dni)
        {
            if (string.IsNullOrWhiteSpace(dni)) return null;
            return dni.Trim();
        }
    }
}
