using AutoMapper;
using Repository.Informes;
using Repository.Pacientes;
using Repository.Turnos;
using Utils.DTOs.Comun;
using Utils.DTOs.Paciente;
using Utils.DTOs.Turno;
using Utils.Exceptions;
using Utils.Helpers;

namespace Service.Pacientes
{
    public class HistorialPacienteService : IHistorialPacienteService
    {
        private const int MaxPageSize = 100;
        private const string FiltroTipoTurno = "turno";
        private const string FiltroTipoInforme = "informe";
        private const string FiltroTipoTodos = "todos";

        private readonly IPacienteRepository _pacienteRepository;
        private readonly ITurnoRepository _turnoRepository;
        private readonly IInformeRepository _informeRepository;
        private readonly IMapper _mapper;

        public HistorialPacienteService(
            IPacienteRepository pacienteRepository,
            ITurnoRepository turnoRepository,
            IInformeRepository informeRepository,
            IMapper mapper)
        {
            _pacienteRepository = pacienteRepository;
            _turnoRepository = turnoRepository;
            _informeRepository = informeRepository;
            _mapper = mapper;
        }

        public async Task<HistorialPacienteResponseDto> GetHistorialAsync(
            int pacienteId,
            int page,
            int pageSize,
            string? tipo = null,
            string? estado = null,
            DateOnly? desde = null,
            DateOnly? hasta = null,
            bool soloConInforme = false)
        {
            ValidatePagination(page, pageSize);
            ValidateFechas(desde, hasta);

            var tipoFiltro = NormalizarTipo(tipo);
            var estadoFiltro = ParsearEstado(estado);

            var paciente = await _pacienteRepository.GetByIdAsync(pacienteId);
            if (paciente == null)
                throw new NotFoundError($"Paciente {pacienteId} no encontrado.");

            var turnos = await _turnoRepository.ObtenerTodosPorPaciente(pacienteId);
            var informes = (await _informeRepository.GetByPacienteIdAsync(pacienteId)).ToList();
            var ahora = DateTime.UtcNow;

            var eventos = new List<HistorialPacienteEventoDto>();

            foreach (var turno in turnos.Where(t => t.FechaHora <= ahora))
            {
                eventos.Add(_mapper.Map<HistorialPacienteEventoDto>(turno));
            }

            foreach (var informe in informes.Where(i => i.Turno == null))
            {
                eventos.Add(_mapper.Map<HistorialPacienteEventoDto>(informe));
            }

            var filtrados = eventos
                .Where(e => CoincideTipo(e, tipoFiltro))
                .Where(e => CoincideEstado(e, estadoFiltro))
                .Where(e => !soloConInforme || e.Informe != null)
                .Where(e => CoincideRango(e, desde, hasta))
                .OrderByDescending(e => e.FechaHora)
                .ThenBy(e => e.Tipo == HistorialPacienteEventoDto.TipoTurno ? 0 : 1)
                .ToList();

            var totalItems = filtrados.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var items = filtrados
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var proximosTurnos = turnos
                .Where(t => t.FechaHora > ahora && t.Estado != EstadoTurno.Cancelado.ToString())
                .OrderBy(t => t.FechaHora)
                .Select(t => _mapper.Map<HistorialPacienteEventoDto>(t))
                .ToList();

            return new HistorialPacienteResponseDto
            {
                Historial = new PageResult<HistorialPacienteEventoDto>
                {
                    Items = items,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                },
                ProximosTurnos = proximosTurnos
            };
        }

        private static bool CoincideTipo(HistorialPacienteEventoDto evento, string tipoFiltro)
        {
            return tipoFiltro switch
            {
                FiltroTipoTurno => evento.Turno != null,
                FiltroTipoInforme => evento.Informe != null,
                _ => true
            };
        }

        private static bool CoincideEstado(HistorialPacienteEventoDto evento, EstadoTurno? estadoFiltro)
        {
            if (!estadoFiltro.HasValue)
                return true;

            if (evento.Turno == null)
                return false;

            return string.Equals(evento.Turno.Estado, estadoFiltro.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private static bool CoincideRango(HistorialPacienteEventoDto evento, DateOnly? desde, DateOnly? hasta)
        {
            if (!desde.HasValue && !hasta.HasValue)
                return true;

            var fecha = evento.Turno == null && evento.Informe != null
                ? evento.Informe.Fecha
                : DateOnly.FromDateTime(DateTimeHelper.UtcALocal(evento.FechaHora));

            if (desde.HasValue && fecha < desde.Value)
                return false;

            if (hasta.HasValue && fecha > hasta.Value)
                return false;

            return true;
        }

        private static string NormalizarTipo(string? tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                return FiltroTipoTodos;

            var normalizado = tipo.Trim().ToLowerInvariant();
            if (normalizado is not (FiltroTipoTodos or FiltroTipoTurno or FiltroTipoInforme))
                throw new ValidationError("El tipo de historial debe ser 'todos', 'turno' o 'informe'.");

            return normalizado;
        }

        private static EstadoTurno? ParsearEstado(string? estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
                return null;

            if (!Enum.TryParse<EstadoTurno>(estado, true, out var resultado) || !Enum.IsDefined(resultado))
                throw new ValidationError("El estado del turno no es válido.");

            return resultado;
        }

        private static void ValidatePagination(int page, int pageSize)
        {
            if (page < 1)
                throw new ValidationError("La página debe ser mayor o igual que 1.");

            if (pageSize < 1 || pageSize > MaxPageSize)
                throw new ValidationError($"El tamaño de página debe estar entre 1 y {MaxPageSize}.");
        }

        private static void ValidateFechas(DateOnly? desde, DateOnly? hasta)
        {
            if (desde.HasValue && hasta.HasValue && desde.Value > hasta.Value)
                throw new ValidationError("La fecha desde no puede ser posterior a la fecha hasta.");
        }
    }
}
