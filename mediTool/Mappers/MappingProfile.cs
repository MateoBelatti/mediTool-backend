using AutoMapper;
using Biblioteca.Entities;
using Utils.DTO;
using Utils.DTOs.Comun;
using Utils.DTOs.Paciente;
using Utils.DTOs.Profesional;
// Branch feature/turnos
using Utils.DTOs.TurnoFijo;
using Utils.DTOs.Turno;
// Branch feature/informe-reuniones
using Utils.DTOs.Informe;
using Utils.DTOs.Reunion;
// Branch feature/notificaciones
using Utils.DTOs.Notificacion;
// Branch feature/documentos
using Utils.DTOs.Documento;
using Utils.Helpers;

namespace mediTool.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapeos de Paciente
            CreateMap<PacienteCreateDto, Paciente>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Activo, opt => opt.Ignore())
                .ForMember(dest => dest.FechaBaja, opt => opt.Ignore());

            CreateMap<PacienteUpdateDto, Paciente>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Activo, opt => opt.Ignore())
                .ForMember(dest => dest.FechaBaja, opt => opt.Ignore());

            CreateMap<Paciente, PacienteResponseDto>();

            // Mapeos de Profesional
            CreateMap<ProfesionalCreateDto, Profesional>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<ProfesionalUpdateDto, Profesional>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore()); // Ignoramos Password al actualizar el perfil básico

            CreateMap<Profesional, ProfesionalResponseDto>();

            // Mapeos de TurnoFijo
            CreateMap<CrearTurnoFijoDto, TurnoFijo>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<EditarTurnoFijoDto, TurnoFijo>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PacienteId, opt => opt.Ignore())
                .ForMember(dest => dest.ProfesionalId, opt => opt.Ignore())
                .ForMember(dest => dest.FechaInicio, opt => opt.Ignore());

            // Mapeos de Turno
            CreateMap<CrearTurnoDto, Turno>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InformeId, opt => opt.Ignore())
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()));

            // Mapeos de salida (response DTOs)
            CreateMap<Paciente, PacienteResumenDto>();
            CreateMap<Profesional, ProfesionalResumenDto>();
            CreateMap<Turno, TurnoResponseDto>();
            CreateMap<TurnoFijo, TurnoFijoResponseDto>();


            // Mapeos de Informe
            CreateMap<InformeCreateDto, Informe>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Paciente, opt => opt.Ignore())
                .ForMember(dest => dest.Profesional, opt => opt.Ignore())
                .ForMember(dest => dest.Turno, opt => opt.Ignore());

            CreateMap<InformeUpdateDto, Informe>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Paciente, opt => opt.Ignore())
                .ForMember(dest => dest.Profesional, opt => opt.Ignore())
                .ForMember(dest => dest.Turno, opt => opt.Ignore());

            CreateMap<Informe, InformeResponseDto>()
                .ForMember(
                    dest => dest.TurnoId,
                    opt => opt.MapFrom(src => src.Turno != null ? src.Turno.Id : (int?)null));

            // Mapeos de Reunion
            CreateMap<ReunionCreateDto, Reunion>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Profesional, opt => opt.Ignore())
                .ForMember(dest => dest.Profesionales, opt => opt.Ignore());

            CreateMap<ReunionUpdateDto, Reunion>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Profesional, opt => opt.Ignore())
                .ForMember(dest => dest.Profesionales, opt => opt.Ignore());

            CreateMap<Reunion, ReunionResponseDto>()
                .ForMember(
                    dest => dest.Profesionales,
                    opt => opt.MapFrom(src => src.Profesionales
                        .Where(rp => rp.Profesional != null)
                        .Select(rp => rp.Profesional!)));

            // Mapeos de Notificacion
            CreateMap<CrearNotificacionDto, Notificacion>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Leida, opt => opt.Ignore())
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
                .ForMember(dest => dest.Profesional, opt => opt.Ignore());

            CreateMap<Notificacion, NotificacionDto>();

            // Mapeos de DocumentoPaciente
            CreateMap<DocumentoPaciente, DocumentoPacienteResponseDto>()
                .ForMember(dest => dest.ProfesionalNombre, opt => opt.MapFrom(src =>
                    src.Profesional == null
                        ? null
                        : $"{src.Profesional.Nombre} {src.Profesional.Apellido}".Trim()));

            // Mapeos de Historial de Paciente
            CreateMap<Turno, HistorialPacienteEventoDto>()
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(_ => HistorialPacienteEventoDto.TipoTurno))
                .ForMember(dest => dest.FechaHora, opt => opt.MapFrom(src => src.FechaHora))
                .ForMember(dest => dest.Profesional, opt => opt.MapFrom(src => src.Profesional))
                .ForMember(dest => dest.Turno, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.Informe, opt => opt.MapFrom(src => src.Informe))
                .AfterMap((src, dest) =>
                {
                    if (dest.Informe != null)
                        dest.Informe.TurnoId ??= src.Id;
                });

            CreateMap<Informe, HistorialPacienteEventoDto>()
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(_ => HistorialPacienteEventoDto.TipoInforme))
                .ForMember(dest => dest.FechaHora, opt => opt.MapFrom(src => DateTimeHelper.LocalAUtc(src.Fecha, TimeSpan.Zero)))
                .ForMember(dest => dest.Profesional, opt => opt.MapFrom(src => src.Profesional))
                .ForMember(dest => dest.Turno, opt => opt.Ignore())
                .ForMember(dest => dest.Informe, opt => opt.MapFrom(src => src));

        }
    }
}
