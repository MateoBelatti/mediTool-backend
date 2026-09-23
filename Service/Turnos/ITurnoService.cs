using Utils.DTOs.Turno;

namespace Service.Turnos
{
    public interface ITurnoService
    {
        Task<TurnoResponseDto> CrearSuelto(CrearTurnoDto dto);
        Task<TurnoResponseDto> ObtenerPorId(int id);
        Task<List<TurnoResponseDto>> ObtenerAgenda(DateTime desde, DateTime hasta, int? profesionalId);
        Task CambiarEstado(int turnoId, EstadoTurno nuevoEstado);
        Task Reprogramar(int turnoId, DateTime nuevaFechaHora);
        Task<TurnoResponseDto> RegistrarAsistencia(int turnoId, bool asistio, bool? justificada, string? observaciones);
        Task<TurnoResponseDto> ActualizarAsistencia(int turnoId, ActualizarAsistenciaDto dto);
        Task<List<TurnoResponseDto>> ObtenerFacturables(int pacienteId, DateTime desde, DateTime hasta);
        Task<ResumenAsistenciaDto> ObtenerResumenPorTurnoFijo(int turnoFijoId);
    }
}