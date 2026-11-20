using Utils.DTOs.Paciente;

namespace Service.Pacientes
{
    public interface IHistorialPacienteService
    {
        Task<HistorialPacienteResponseDto> GetHistorialAsync(
            int pacienteId,
            int page,
            int pageSize,
            string? tipo = null,
            string? estado = null,
            DateOnly? desde = null,
            DateOnly? hasta = null,
            bool soloConInforme = false);
    }
}
