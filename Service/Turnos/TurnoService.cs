using AutoMapper;
using Biblioteca.Entities;
using Biblioteca.Repository;
using Repository.Pacientes;
using Repository.Profesionales;
using Repository.Turnos;
using Utils.DTOs.Comun;
using Utils.DTOs.Turno;
using Utils.Exceptions;

namespace Service.Turnos
{
    public class TurnoService : ITurnoService
    {
        private const int MaxPageSize = 1000;

        private readonly ITurnoRepository _turnoRepository;
        private readonly IPacienteRepository _pacienteRepository;
        private readonly IProfesionalRepository _profesionalRepository;
        private readonly IMapper _mapper;

        private static readonly IReadOnlyDictionary<EstadoTurno, EstadoTurno[]> TransicionesPermitidas =
            new Dictionary<EstadoTurno, EstadoTurno[]>
            {
                [EstadoTurno.Pendiente] = [EstadoTurno.Cancelado, EstadoTurno.Reprogramado],
                [EstadoTurno.Reprogramado] = [EstadoTurno.Cancelado, EstadoTurno.Reprogramado],
                [EstadoTurno.Presente] = [],
                [EstadoTurno.Ausente] = [],
                [EstadoTurno.Cancelado] = []
            };

        public TurnoService(
            ITurnoRepository turnoRepository,
            IPacienteRepository pacienteRepository,
            IProfesionalRepository profesionalRepository,
            IMapper mapper)
        {
            _turnoRepository = turnoRepository;
            _pacienteRepository = pacienteRepository;
            _profesionalRepository = profesionalRepository;
            _mapper = mapper;
        }

        public async Task CambiarEstado(int turnoId, EstadoTurno nuevoEstado)
        {
            var turno = await _turnoRepository.ObtenerPorId(turnoId);
            if (turno == null)
                throw new NotFoundError($"Turno {turnoId} no encontrado");

            ValidarTransicion(turnoId, turno.Estado, nuevoEstado);

            turno.Estado = nuevoEstado.ToString();
            await _turnoRepository.Actualizar(turno);
            await _turnoRepository.GuardarCambios();
        }

        private static void ValidarTransicion(int turnoId, string? estadoActual, EstadoTurno nuevoEstado)
        {
            var estadoOrigen = Enum.TryParse<EstadoTurno>(estadoActual, true, out var origen)
                && Enum.IsDefined(origen)
                    ? origen
                    : (EstadoTurno?)null;

            if (estadoOrigen is null
                || !TransicionesPermitidas.TryGetValue(estadoOrigen.Value, out var permitidos)
                || !permitidos.Contains(nuevoEstado))
            {
                throw new ConflictError($"No se puede cambiar el turno {turnoId} de estado {estadoActual} a {nuevoEstado}.");
            }
        }

        private static void ValidatePagination(int page, int pageSize)
        {
            if (page < 1)
                throw new ValidationError("La página debe ser mayor o igual que 1.");

            if (pageSize < 1 || pageSize > MaxPageSize)
                throw new ValidationError($"El tamaño de página debe estar entre 1 y {MaxPageSize}.");
        }

        public async Task<TurnoResponseDto> CrearSuelto(CrearTurnoDto dto)
        {
            var fechaHoraUtc = dto.FechaHora.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dto.FechaHora, DateTimeKind.Utc) : dto.FechaHora.ToUniversalTime();
            if (fechaHoraUtc < DateTime.UtcNow)
                throw new ValidationError("No se puede crear un turno con fecha pasada.");

            if (await _pacienteRepository.GetByIdAsync(dto.PacienteId) == null)
                throw new ValidationError($"El Paciente con Id {dto.PacienteId} no existe.");

            if (await _profesionalRepository.GetByIdAsync(dto.ProfesionalId) == null)
                throw new ValidationError($"El Profesional con Id {dto.ProfesionalId} no existe.");

            if (!await _pacienteRepository.IsVinculadoAsync(dto.PacienteId, dto.ProfesionalId))
                throw new ConflictError(ApplicationDbContextExtensions.MensajeNoVinculado);

            if (await _turnoRepository.ExisteSolapamiento(dto.ProfesionalId, dto.FechaHora, dto.DuracionMin))
            {
                throw new ConflictError("Ya existe un turno para este profesional en ese horario.");
            }

            var turno = _mapper.Map<Turno>(dto);
            turno.FechaHora = turno.FechaHora.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(turno.FechaHora, DateTimeKind.Utc) : turno.FechaHora.ToUniversalTime();
            turno.FechaRegistro = DateTime.UtcNow;
            await _turnoRepository.Agregar(turno);
            await _turnoRepository.GuardarCambios();

            return _mapper.Map<TurnoResponseDto>(turno);
        }

        public async Task<PageResult<TurnoResponseDto>> ObtenerAgenda(DateTime desde, DateTime hasta, int page, int pageSize, int? profesionalId)
        {
            ValidatePagination(page, pageSize);

            var result = await _turnoRepository.ObtenerPorRangoFecha(desde, hasta, page, pageSize, profesionalId);
            var items = _mapper.Map<IEnumerable<TurnoResponseDto>>(result.Items).ToList();
            var totalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize);

            return new PageResult<TurnoResponseDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = result.TotalItems,
                TotalPages = totalPages
            };
        }

        public async Task<TurnoResponseDto> ObtenerPorId(int id)
        {
            var turno = await _turnoRepository.ObtenerPorId(id);
            if (turno == null)
                throw new NotFoundError($"Turno {id} no encontrado");
            
            return _mapper.Map<TurnoResponseDto>(turno);
        }

        public async Task Reprogramar(int turnoId, DateTime nuevaFechaHora)
        {
            var turno = await _turnoRepository.ObtenerPorId(turnoId);
            if (turno == null)
                throw new NotFoundError($"Turno {turnoId} no encontrado");

            if (turno.Estado != EstadoTurno.Pendiente.ToString() && turno.Estado != EstadoTurno.Reprogramado.ToString())
                throw new ConflictError($"No se puede reprogramar el turno {turnoId} porque está en estado {turno.Estado}.");

            if (await _turnoRepository.ExisteSolapamiento(turno.ProfesionalId, nuevaFechaHora, turno.DuracionMin, turnoId))
            {
                throw new ConflictError("Ya existe un turno para este profesional en el nuevo horario.");
            }

            turno.FechaHora = nuevaFechaHora.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(nuevaFechaHora, DateTimeKind.Utc) : nuevaFechaHora.ToUniversalTime();
            turno.Estado = EstadoTurno.Reprogramado.ToString();
            await _turnoRepository.Actualizar(turno);
            await _turnoRepository.GuardarCambios();
        }

        public async Task<TurnoResponseDto> RegistrarAsistencia(int turnoId, bool asistio, bool? justificada, string? observaciones)
        {
            var turno = await _turnoRepository.ObtenerPorId(turnoId);
            if (turno == null)
                throw new NotFoundError($"Turno {turnoId} no encontrado");

            if (turno.Estado == EstadoTurno.Presente.ToString() || turno.Estado == EstadoTurno.Ausente.ToString())
                throw new ConflictError($"Ya existe una asistencia registrada para el turno {turnoId}");

            turno.Estado = asistio ? EstadoTurno.Presente.ToString() : EstadoTurno.Ausente.ToString();
            turno.Justificada = justificada ?? false;
            turno.Facturable = asistio || (justificada ?? false);
            turno.FechaRegistroAsistencia = DateTime.UtcNow;
            turno.Observaciones = observaciones;

            await _turnoRepository.Actualizar(turno);
            await _turnoRepository.GuardarCambios();

            return _mapper.Map<TurnoResponseDto>(turno);
        }

        public async Task<TurnoResponseDto> ActualizarAsistencia(int turnoId, ActualizarAsistenciaDto dto)
        {
            var turno = await _turnoRepository.ObtenerPorId(turnoId);
            if (turno == null)
                throw new NotFoundError($"Turno {turnoId} no encontrado");

            if (turno.Estado != EstadoTurno.Presente.ToString() && turno.Estado != EstadoTurno.Ausente.ToString())
                throw new NotFoundError($"No hay asistencia registrada para el turno {turnoId}");

            turno.Estado = dto.Asistio ? EstadoTurno.Presente.ToString() : EstadoTurno.Ausente.ToString();
            turno.Justificada = dto.Justificada;
            turno.Facturable = dto.Asistio || dto.Justificada;
            turno.Observaciones = dto.Observaciones;

            await _turnoRepository.Actualizar(turno);
            await _turnoRepository.GuardarCambios();

            return _mapper.Map<TurnoResponseDto>(turno);
        }

        public async Task<List<TurnoResponseDto>> ObtenerFacturables(int pacienteId, DateTime desde, DateTime hasta)
        {
            var turnos = await _turnoRepository.ObtenerFacturables(pacienteId, desde, hasta);
            return _mapper.Map<List<TurnoResponseDto>>(turnos);
        }

        public async Task<ResumenAsistenciaDto> ObtenerResumenPorTurnoFijo(int turnoFijoId)
        {
            var turnos = await _turnoRepository.ObtenerFuturosPorTurnoFijo(turnoFijoId, DateTime.MinValue);

            var totalAsistencias = turnos.Count(t => t.Estado == EstadoTurno.Presente.ToString());
            var totalJustificadas = turnos.Count(t => t.Estado == EstadoTurno.Ausente.ToString() && t.Justificada);
            var totalInjustificadas = turnos.Count(t => t.Estado == EstadoTurno.Ausente.ToString() && !t.Justificada);

            return new ResumenAsistenciaDto
            {
                TurnoFijoId = turnoFijoId,
                TotalTurnos = turnos.Count,
                TotalAsistencias = totalAsistencias,
                TotalAusenciasJustificadas = totalJustificadas,
                TotalAusenciasInjustificadas = totalInjustificadas
            };
        }
    }
}
