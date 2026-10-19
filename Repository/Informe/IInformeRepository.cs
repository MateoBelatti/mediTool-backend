using Biblioteca.Entities;

namespace Repository.Informes
{
    public interface IInformeRepository
    {
        Task<IEnumerable<Informe>> GetAllAsync();
        Task<Informe?> GetByIdAsync(int id);
        Task<IEnumerable<Informe>> GetByProfesionalIdAsync(int profesionalId);
        Task<Informe?> GetByTurnoIdAsync(int turnoId);
        Task<bool> ExisteParaTurnoAsync(int turnoId, int? excluirInformeId = null);
        Task<Informe> AddAsync(Informe entity);
        Task<Informe> UpdateAsync(Informe entity);
        Task<bool> DeleteAsync(Informe entity);
        Task GuardarCambios();
    }
}
