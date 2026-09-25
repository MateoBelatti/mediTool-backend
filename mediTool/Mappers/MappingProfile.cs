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
                .ForMember(dest => dest.Profesional, opt => opt.Ignore());

            CreateMap<InformeUpdateDto, Informe>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Paciente, opt => opt.Ignore())
                .ForMember(dest => dest.Profesional, opt => opt.Ignore());

            CreateMap<Informe, InformeResponseDto>();

            // Mapeos de Reunion
            CreateMap<ReunionCreateDto, Reunion>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Profesional, opt => opt.Ignore());

            CreateMap<ReunionUpdateDto, Reunion>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Profesional, opt => opt.Ignore());

            CreateMap<Reunion, ReunionResponseDto>();

        }
    }
}
