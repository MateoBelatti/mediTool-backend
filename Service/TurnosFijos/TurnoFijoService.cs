using AutoMapper;
using Biblioteca.Entities;
using Biblioteca.Repository;
using Repository.Pacientes;
using Repository.TurnosFijos;
using Service.Turnos;
using Utils.DTOs.TurnoFijo;
using Utils.Exceptions;

namespace Service.TurnosFijos
{
    public class TurnoFijoService : ITurnoFijoService
    {
        private readonly ITurnoFijoRepository _turnoFijoRepository;
        private readonly IGenerarInstanciasTurnoService _generarInstanciasService;
        private readonly IPacienteRepository _pacienteRepository;
        private readonly IMapper _mapper;

        public TurnoFijoService(
            ITurnoFijoRepository turnoFijoRepository, 
            IGenerarInstanciasTurnoService generarInstanciasService,
            IPacienteRepository pacienteRepository,
            IMapper mapper)
        {
            _turnoFijoRepository = turnoFijoRepository;
            _generarInstanciasService = generarInstanciasService;
            _pacienteRepository = pacienteRepository;
            _mapper = mapper;
        }

        public async Task<TurnoFijoResponseDto> Crear(CrearTurnoFijoDto dto)
        {
            var turnoFijo = _mapper.Map<TurnoFijo>(dto);
            ValidarReglas(turnoFijo);

            if (!await _pacienteRepository.IsVinculadoAsync(turnoFijo.PacienteId, turnoFijo.ProfesionalId))
                throw new ConflictError(ApplicationDbContextExtensions.MensajeNoVinculado);

            await _turnoFijoRepository.Agregar(turnoFijo);
            await _turnoFijoRepository.GuardarCambios();
            
            // Generar instancias para los próximos 3 meses
            await _generarInstanciasService.GenerarInstancias(turnoFijo, DateTime.Now.AddMonths(3));
            
            return _mapper.Map<TurnoFijoResponseDto>(turnoFijo);
        }

        public async Task Desactivar(int turnoFijoId)
        {
            var turnoFijo = await _turnoFijoRepository.ObtenerPorId(turnoFijoId);
            if (turnoFijo == null)
                throw new NotFoundError($"TurnoFijo {turnoFijoId} no encontrado");

            turnoFijo.Activo = false;
            await _turnoFijoRepository.Actualizar(turnoFijo);
            await _turnoFijoRepository.GuardarCambios();
            
            // Cancelar los turnos futuros asociados a esta regla
            await _generarInstanciasService.CancelarInstanciasFuturas(turnoFijoId);
        }

        public async Task<TurnoFijoResponseDto> Editar(int turnoFijoId, EditarTurnoFijoDto dto)
        {
            var turnoFijo = await _turnoFijoRepository.ObtenerPorId(turnoFijoId);
            if (turnoFijo == null)
                throw new NotFoundError($"TurnoFijo {turnoFijoId} no encontrado");

            _mapper.Map(dto, turnoFijo);
            ValidarReglas(turnoFijo);
            await _turnoFijoRepository.Actualizar(turnoFijo);
            await _turnoFijoRepository.GuardarCambios();
            
            // Actualizar los turnos que ya se habían programado a futuro con la vieja regla
            await _generarInstanciasService.ActualizarInstanciasFuturas(turnoFijo);
            
            return _mapper.Map<TurnoFijoResponseDto>(turnoFijo);
        }

        public async Task<List<TurnoFijoResponseDto>> ListarPorProfesional(int profesionalId)
        {
            var turnosFijos = await _turnoFijoRepository.ObtenerPorProfesional(profesionalId);
            return _mapper.Map<List<TurnoFijoResponseDto>>(turnosFijos);
        }

        public async Task<TurnoFijoResponseDto?> ObtenerPorId(int id)
        {
            var turnoFijo = await _turnoFijoRepository.ObtenerPorId(id);
            if (turnoFijo == null) return null;
            return _mapper.Map<TurnoFijoResponseDto>(turnoFijo);
        }

        private static void ValidarReglas(TurnoFijo turnoFijo)
        {
            if (turnoFijo.DiaSemana is < 0 or > 6)
                throw new ValidationError(
                    $"El DiaSemana debe estar entre 0 (Domingo) y 6 (Sábado). Valor recibido: {turnoFijo.DiaSemana}.");

            if (turnoFijo.DuracionMin <= 0)
                throw new ValidationError(
                    $"La DuracionMin debe ser mayor a 0. Valor recibido: {turnoFijo.DuracionMin}.");

            if (turnoFijo.FechaFin.HasValue && turnoFijo.FechaInicio > turnoFijo.FechaFin.Value)
                throw new ValidationError(
                    $"La FechaInicio ({turnoFijo.FechaInicio}) debe ser menor o igual a la FechaFin ({turnoFijo.FechaFin.Value}).");
        }
    }
}
