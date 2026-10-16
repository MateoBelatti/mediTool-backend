using Biblioteca.Entities;

namespace Service.Tests.TestSupport;

/// <summary>
/// Construye entidades de dominio con valores por defecto sensatos para los tests.
/// </summary>
internal static class TestObjects
{
    public const int PacienteId = 1;
    public const int ProfesionalId = 2;
    public const int TurnoFijoId = 10;

    public static Paciente Paciente(int id = PacienteId, bool activo = true) => new()
    {
        Id = id,
        Nombre = "Juan",
        Apellido = "Perez",
        Activo = activo
    };

    public static Profesional Profesional(int id = ProfesionalId) => new()
    {
        Id = id,
        Nombre = "Ana",
        Apellido = "Gomez",
        Email = "ana@meditool.test",
        Password = "hash",
        Rol = Utils.Enums.Rol.Profesional
    };

    public static Turno Turno(
        int id = 1,
        string estado = "Pendiente",
        DateTime? fechaHora = null,
        int duracionMin = 30,
        int pacienteId = PacienteId,
        int profesionalId = ProfesionalId,
        int? turnoFijoId = null) => new()
    {
        Id = id,
        PacienteId = pacienteId,
        ProfesionalId = profesionalId,
        TurnoFijoId = turnoFijoId,
        FechaHora = fechaHora ?? new DateTime(2035, 1, 8, 13, 0, 0, DateTimeKind.Utc),
        DuracionMin = duracionMin,
        Estado = estado,
        FechaRegistro = new DateTime(2035, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    public static TurnoFijo TurnoFijo(
        int id = TurnoFijoId,
        int diaSemana = 1,
        TimeSpan? hora = null,
        int duracionMin = 30,
        DateOnly? fechaInicio = null,
        DateOnly? fechaFin = null,
        bool activo = true,
        int pacienteId = PacienteId,
        int profesionalId = ProfesionalId) => new()
    {
        Id = id,
        PacienteId = pacienteId,
        ProfesionalId = profesionalId,
        DiaSemana = diaSemana,
        Hora = hora ?? new TimeSpan(10, 0, 0),
        DuracionMin = duracionMin,
        FechaInicio = fechaInicio ?? new DateOnly(2035, 1, 1),
        FechaFin = fechaFin,
        Activo = activo
    };

    public static Notificacion Notificacion(
        int id = 1,
        int profesionalId = ProfesionalId,
        bool leida = false,
        Utils.Enums.TipoNotificacion tipo = Utils.Enums.TipoNotificacion.Sistema) => new()
    {
        Id = id,
        ProfesionalId = profesionalId,
        Tipo = tipo,
        Titulo = "Título de prueba",
        Mensaje = "Mensaje de prueba",
        Leida = leida,
        FechaCreacion = new DateTime(2035, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    };
}
