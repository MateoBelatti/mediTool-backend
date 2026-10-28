using Biblioteca.Entities;

namespace Repository.Reuniones
{
    public interface IReunionRepository
    {
        Task<IEnumerable<Reunion>> GetAllAsync();
        Task<Reunion?> GetByIdAsync(int id);
        Task<IEnumerable<Reunion>> GetByProfesionalIdAsync(int profesionalId);
        Task<Reunion> AddAsync(Reunion entity);
        Task<Reunion> UpdateAsync(Reunion entity);
        Task<bool> DeleteAsync(Reunion entity);
        Task GuardarCambios();
    }
}
