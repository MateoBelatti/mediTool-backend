using Biblioteca.Entities;

namespace Repository.TurnosFijos
{
    public interface ITurnoFijoRepository
    {
        Task<TurnoFijo?> ObtenerPorId(int id);
        Task<List<TurnoFijo>> ObtenerActivos();
        Task<List<TurnoFijo>> ObtenerPorProfesional(int profesionalId);
        Task Agregar(TurnoFijo turnoFijo);
        Task Actualizar(TurnoFijo turnoFijo);
        Task GuardarCambios();
    }
}
