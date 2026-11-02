using AutoMapper;
using Biblioteca.Entities;
using Repository.Profesionales;
using Repository.Reuniones;
using Utils.DTOs.Reunion;
using Utils.Exceptions;
using Utils.Helpers;

namespace Service.Reuniones
{
    public class ReunionService : IReunionService
    {
        private readonly IReunionRepository _reunionRepository;
        private readonly IProfesionalRepository _profesionalRepository;
        private readonly IMapper _mapper;

        public ReunionService(
            IReunionRepository reunionRepository,
            IProfesionalRepository profesionalRepository,
            IMapper mapper)
        {
            _reunionRepository = reunionRepository;
            _profesionalRepository = profesionalRepository;
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
            var profesionales = await ResolverProfesionalesAsync(dto.ProfesionalesIds);

            var reunion = _mapper.Map<Reunion>(dto);
            reunion.FechaHora = DateTimeHelper.NormalizarAUtc(reunion.FechaHora);
            reunion.Profesionales = profesionales
                .Select(p => new ReunionProfesional
                {
                    ProfesionalId = p.Id,
                    Profesional = p
                })
                .ToList();

            var created = await _reunionRepository.AddAsync(reunion);
            await _reunionRepository.GuardarCambios();
            return _mapper.Map<ReunionResponseDto>(created);
        }

        public async Task<ReunionResponseDto?> UpdateAsync(int id, ReunionUpdateDto dto)
        {
            var existing = await _reunionRepository.GetByIdAsync(id);
            if (existing == null) return null;

            var profesionales = await ResolverProfesionalesAsync(dto.ProfesionalesIds);

            _mapper.Map(dto, existing);
            existing.FechaHora = DateTimeHelper.NormalizarAUtc(existing.FechaHora);

            var idsNuevos = profesionales.Select(p => p.Id).ToHashSet();

            var aEliminar = existing.Profesionales
                .Where(rp => !idsNuevos.Contains(rp.ProfesionalId))
                .ToList();
            foreach (var asignacion in aEliminar)
                existing.Profesionales.Remove(asignacion);

            var idsActuales = existing.Profesionales
                .Select(rp => rp.ProfesionalId)
                .ToHashSet();
            foreach (var profesional in profesionales.Where(p => !idsActuales.Contains(p.Id)))
            {
                existing.Profesionales.Add(new ReunionProfesional
                {
                    ReunionId = existing.Id,
                    ProfesionalId = profesional.Id,
                    Profesional = profesional
                });
            }

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

        private async Task<List<Profesional>> ResolverProfesionalesAsync(IEnumerable<int>? ids)
        {
            var idsDistintos = (ids ?? Enumerable.Empty<int>())
                .Distinct()
                .ToList();

            if (idsDistintos.Count == 0)
                throw new ValidationError("Seleccioná al menos un profesional para la reunión.");

            if (idsDistintos.Any(id => id <= 0))
                throw new ValidationError("Los ids de profesionales deben ser mayores que 0.");

            var profesionales = (await _profesionalRepository.GetByIdsAsync(idsDistintos)).ToList();
            if (profesionales.Count != idsDistintos.Count)
            {
                var encontrados = profesionales.Select(p => p.Id).ToHashSet();
                var faltantes = idsDistintos.Where(id => !encontrados.Contains(id));
                throw new ValidationError(
                    $"No existen los profesionales con id: {string.Join(", ", faltantes)}.");
            }

            return profesionales;
        }
    }
}
