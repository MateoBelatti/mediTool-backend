using Utils.DTOs.Comun;

namespace Utils.DTOs.Paciente
{
    public class HistorialPacienteResponseDto
    {
        public PageResult<HistorialPacienteEventoDto> Historial { get; set; } = new();
        public IEnumerable<HistorialPacienteEventoDto> ProximosTurnos { get; set; } = Array.Empty<HistorialPacienteEventoDto>();
    }
}
