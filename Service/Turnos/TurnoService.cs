using AutoMapper;
using Biblioteca.Entities;
using Biblioteca.Repository;
using Repository.Pacientes;
using Repository.Profesionales;
using Repository.Turnos;
using Utils.DTOs.Turno;
using Utils.Exceptions;

namespace Service.Turnos
{
    public class TurnoService : ITurnoService
    {
        private readonly ITurnoRepository _turnoRepository;
        private readonly IPacienteRepository _pacienteRepository;
        private readonly IProfesionalRepository _profesionalRepository;
        private readonly IMapper _mapper;

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

            turno.Estado = nuevoEstado.ToString();
            await _turnoRepository.Actualizar(turno);
            await _turnoRepository.GuardarCambios();
        }

        public async Task<TurnoResponseDto> CrearSuelto(CrearTurnoDto dto)
        {
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

        public async Task<List<TurnoResponseDto>> ObtenerAgenda(DateTime desde, DateTime hasta, int? profesionalId)
        {
            var turnos = await _turnoRepository.ObtenerPorRangoFecha(desde, hasta, profesionalId);
            return _mapper.Map<List<TurnoResponseDto>>(turnos);
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
            turno.FechaRegistroAsistencia = DateTime.Now;
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
