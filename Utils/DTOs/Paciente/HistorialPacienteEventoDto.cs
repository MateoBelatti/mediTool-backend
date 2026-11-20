using Utils.DTOs.Comun;
using Utils.DTOs.Informe;
using Utils.DTOs.Turno;

namespace Utils.DTOs.Paciente
{
    public class HistorialPacienteEventoDto
    {
        public const string TipoTurno = "Turno";
        public const string TipoInforme = "Informe";

        public string Tipo { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public ProfesionalResumenDto? Profesional { get; set; }
        public TurnoResponseDto? Turno { get; set; }
        public InformeResponseDto? Informe { get; set; }
    }
}
