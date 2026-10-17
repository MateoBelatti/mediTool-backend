using AutoMapper;
using Biblioteca.Entities;
using NSubstitute;
using Utils.DTOs.Notificacion;
using Utils.DTOs.Turno;
using Utils.DTOs.TurnoFijo;

namespace Service.Tests.TestSupport;

/// <summary>
/// Doble de <see cref="IMapper"/> que replica sólo las conversiones que usan los
/// servicios bajo prueba, evitando depender del perfil de AutoMapper del proyecto API.
/// </summary>
internal static class MapperStub
{
    public static IMapper Create()
    {
        var mapper = Substitute.For<IMapper>();

        mapper.Map<Turno>(Arg.Any<CrearTurnoDto>())
            .Returns(ci => ToTurno(ci.Arg<CrearTurnoDto>()));

        mapper.Map<TurnoResponseDto>(Arg.Any<Turno>())
            .Returns(ci => ToTurnoDto(ci.Arg<Turno>()));

        mapper.Map<IEnumerable<TurnoResponseDto>>(Arg.Any<IEnumerable<Turno>>())
            .Returns(ci => ci.Arg<IEnumerable<Turno>>().Select(ToTurnoDto).ToList());

        mapper.Map<List<TurnoResponseDto>>(Arg.Any<List<Turno>>())
            .Returns(ci => ci.Arg<List<Turno>>().Select(ToTurnoDto).ToList());

        mapper.Map<TurnoFijo>(Arg.Any<CrearTurnoFijoDto>())
            .Returns(ci => ToTurnoFijo(ci.Arg<CrearTurnoFijoDto>()));

        mapper.Map<TurnoFijoResponseDto>(Arg.Any<TurnoFijo>())
            .Returns(ci => ToTurnoFijoDto(ci.Arg<TurnoFijo>()));

        mapper.Map<List<TurnoFijoResponseDto>>(Arg.Any<List<TurnoFijo>>())
            .Returns(ci => ci.Arg<List<TurnoFijo>>().Select(ToTurnoFijoDto).ToList());

        mapper.Map(Arg.Any<EditarTurnoFijoDto>(), Arg.Any<TurnoFijo>())
            .Returns(ci =>
            {
                AplicarEdicion(ci.Arg<EditarTurnoFijoDto>(), ci.Arg<TurnoFijo>());
                return ci.Arg<TurnoFijo>();
            });

        mapper.Map<Notificacion>(Arg.Any<CrearNotificacionDto>())
            .Returns(ci => ToNotificacion(ci.Arg<CrearNotificacionDto>()));

        mapper.Map<NotificacionDto>(Arg.Any<Notificacion>())
            .Returns(ci => ToNotificacionDto(ci.Arg<Notificacion>()));

        mapper.Map<IEnumerable<NotificacionDto>>(Arg.Any<IEnumerable<Notificacion>>())
            .Returns(ci => ci.Arg<IEnumerable<Notificacion>>().Select(ToNotificacionDto).ToList());

        return mapper;
    }

    private static Turno ToTurno(CrearTurnoDto dto) => new()
    {
        TurnoFijoId = dto.TurnoFijoId,
        PacienteId = dto.PacienteId,
        ProfesionalId = dto.ProfesionalId,
        FechaHora = dto.FechaHora,
        DuracionMin = dto.DuracionMin,
        Estado = dto.Estado.ToString(),
        Justificada = dto.Justificada,
        Facturable = dto.Facturable,
        Observaciones = dto.Observaciones
    };

    private static TurnoResponseDto ToTurnoDto(Turno turno) => new()
    {
        Id = turno.Id,
        TurnoFijoId = turno.TurnoFijoId,
        PacienteId = turno.PacienteId,
        ProfesionalId = turno.ProfesionalId,
        FechaHora = turno.FechaHora,
        DuracionMin = turno.DuracionMin,
        Estado = turno.Estado,
        Justificada = turno.Justificada,
        Facturable = turno.Facturable,
        FechaRegistro = turno.FechaRegistro,
        FechaRegistroAsistencia = turno.FechaRegistroAsistencia,
        Observaciones = turno.Observaciones
    };

    private static TurnoFijo ToTurnoFijo(CrearTurnoFijoDto dto) => new()
    {
        PacienteId = dto.PacienteId,
        ProfesionalId = dto.ProfesionalId,
        DiaSemana = dto.DiaSemana,
        Hora = dto.Hora,
        DuracionMin = dto.DuracionMin,
        FechaInicio = dto.FechaInicio,
        FechaFin = dto.FechaFin,
        Activo = dto.Activo
    };

    private static TurnoFijoResponseDto ToTurnoFijoDto(TurnoFijo regla) => new()
    {
        Id = regla.Id,
        PacienteId = regla.PacienteId,
        ProfesionalId = regla.ProfesionalId,
        DiaSemana = regla.DiaSemana,
        Hora = regla.Hora,
        DuracionMin = regla.DuracionMin,
        FechaInicio = regla.FechaInicio,
        FechaFin = regla.FechaFin,
        Activo = regla.Activo
    };

    private static void AplicarEdicion(EditarTurnoFijoDto dto, TurnoFijo regla)
    {
        regla.DiaSemana = dto.DiaSemana;
        regla.Hora = dto.Hora;
        regla.DuracionMin = dto.DuracionMin;
        regla.FechaFin = dto.FechaFin;
        regla.Activo = dto.Activo;
    }

    private static Notificacion ToNotificacion(CrearNotificacionDto dto) => new()
    {
        ProfesionalId = dto.ProfesionalId,
        Tipo = dto.Tipo,
        Titulo = dto.Titulo,
        Mensaje = dto.Mensaje
    };

    private static NotificacionDto ToNotificacionDto(Notificacion notificacion) => new()
    {
        Id = notificacion.Id,
        ProfesionalId = notificacion.ProfesionalId,
        Tipo = notificacion.Tipo,
        Titulo = notificacion.Titulo,
        Mensaje = notificacion.Mensaje,
        Leida = notificacion.Leida,
        FechaCreacion = notificacion.FechaCreacion
    };
}
