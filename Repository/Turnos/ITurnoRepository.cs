using Biblioteca.Entities;

namespace Repository.Turnos
{
    public interface ITurnoRepository
    {
        Task<Turno?> ObtenerPorId(int id);
        Task<List<Turno>> ObtenerPorRangoFecha(DateTime desde, DateTime hasta, int? profesionalId = null);
        Task<List<Turno>> ObtenerFacturables(int pacienteId, DateTime desde, DateTime hasta);
        Task<Turno?> ObtenerUltimoPorTurnoFijo(int turnoFijoId);
        Task<List<Turno>> ObtenerFuturosPorTurnoFijo(int turnoFijoId, DateTime desde);
        Task<bool> ExisteSolapamiento(int profesionalId, DateTime fechaHora, int duracionMin, int? excluirTurnoId = null);
        Task Agregar(Turno turno);
        Task Actualizar(Turno turno);
        Task ActualizarRango(List<Turno> turnos);
        Task GuardarCambios();
    }
}
