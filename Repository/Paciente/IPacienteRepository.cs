using Biblioteca.Entities;

namespace Repository.Pacientes
{
    public interface IPacienteRepository
    {
        Task<IEnumerable<Paciente>> GetAllAsync();
        Task<IEnumerable<Paciente>> GetAllVinculadosAsync(int profesionalId);
        Task<(IEnumerable<Paciente> Items, int TotalItems)> GetAllPagedAsync(int page, int pageSize, int? profesionalId = null);
        Task<bool> IsVinculadoAsync(int pacienteId, int profesionalId);
        Task VincularAsync(int pacienteId, int profesionalId);
        Task<Paciente?> GetAsync(Paciente entity);
        Task<Paciente> AddAsync(Paciente entity);
        Task<Paciente> UpdateAsync(Paciente entity);
        Task<bool> DeleteAsync(Paciente entity);
        Task<Paciente?> GetByIdAsync(int id);
        Task<Paciente?> GetByDniAsync(string dni);
        Task<Paciente?> GetByEmailAsync(string email);
        Task<IEnumerable<Paciente>> GetByObraSocialAsync(string obraSocial);
        Task GuardarCambios();
    }
}
