using Utils.DTOs.TurnoFijo;

namespace Service.TurnosFijos
{
    public interface ITurnoFijoService
    {
        Task<TurnoFijoResponseDto> Crear(CrearTurnoFijoDto dto);
        Task<TurnoFijoResponseDto> Editar(int turnoFijoId, EditarTurnoFijoDto dto);
        Task Desactivar(int turnoFijoId);
        Task<List<TurnoFijoResponseDto>> ListarPorProfesional(int profesionalId);
        Task<TurnoFijoResponseDto?> ObtenerPorId(int id);
    }
}