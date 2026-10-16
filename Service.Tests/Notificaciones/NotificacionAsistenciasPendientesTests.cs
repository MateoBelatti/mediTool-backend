using Biblioteca.Entities;
using Biblioteca.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Repository.Notificaciones;
using Repository.Turnos;
using Service.Tests.TestSupport;
using Utils.Enums;

namespace Service.Tests.Notificaciones;

public class NotificacionAsistenciasPendientesTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly TurnoRepository _turnoRepository;
    private readonly NotificacionRepository _notificacionRepository;

    public NotificacionAsistenciasPendientesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _dbContext = new ApplicationDbContext(options);
        _turnoRepository = new TurnoRepository(_dbContext);
        _notificacionRepository = new NotificacionRepository(_dbContext);
    }

    private void AgregarTurno(int profesionalId, string estado, DateTime fechaHora)
    {
        _dbContext.Turnos.Add(new Turno
        {
            ProfesionalId = profesionalId,
            PacienteId = TestObjects.PacienteId,
            Estado = estado,
            FechaHora = fechaHora,
            DuracionMin = 30,
            FechaRegistro = DateTime.UtcNow
        });
    }

    // ----- ContarPendientesPorProfesional -----

    [Fact]
    public async Task ContarPendientesPorProfesional_TurnosViejosYPendientes_AgrupaPorProfesional()
    {
        // Arrange: profesional 1 tiene 3 pendientes viejos, profesional 2 tiene 1
        var ayer = DateTime.UtcNow.AddDays(-1);
        AgregarTurno(TestObjects.ProfesionalId, "Pendiente", ayer.AddDays(-1));
        AgregarTurno(TestObjects.ProfesionalId, "Pendiente", ayer.AddDays(-2));
        AgregarTurno(TestObjects.ProfesionalId, "Pendiente", ayer.AddDays(-3));
        AgregarTurno(99, "Pendiente", ayer);

        // Futuro pendiente y viejo cancelado: no cuentan
        AgregarTurno(TestObjects.ProfesionalId, "Pendiente", DateTime.UtcNow.AddDays(1));
        AgregarTurno(TestObjects.ProfesionalId, "Cancelado", ayer);
        await _dbContext.SaveChangesAsync();

        // Act
        var resultado = await _turnoRepository.ContarPendientesPorProfesional(DateTime.UtcNow);

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Should().Contain(r => r.ProfesionalId == TestObjects.ProfesionalId && r.CantidadPendientes == 3);
        resultado.Should().Contain(r => r.ProfesionalId == 99 && r.CantidadPendientes == 1);
    }

    [Fact]
    public async Task ContarPendientesPorProfesional_SinPendientes_DevuelveListaVacia()
    {
        // Arrange
        AgregarTurno(TestObjects.ProfesionalId, "Presente", DateTime.UtcNow.AddDays(-1));
        await _dbContext.SaveChangesAsync();

        // Act
        var resultado = await _turnoRepository.ContarPendientesPorProfesional(DateTime.UtcNow);

        // Assert
        resultado.Should().BeEmpty();
    }

    // ----- ExisteNotificacionHoy -----

    [Fact]
    public async Task ExisteNotificacionHoy_ConNotificacionDeHoyDelMismoTipo_DevuelveTrue()
    {
        // Arrange
        _notificacionRepository.Crear(new Notificacion
        {
            ProfesionalId = TestObjects.ProfesionalId,
            Tipo = TipoNotificacion.TurnosSinAsistencia,
            Titulo = "Turnos pendientes de asistencia",
            Mensaje = "Tenés 3 turnos pendientes de registrar asistencia.",
            FechaCreacion = DateTime.UtcNow
        });
        await _notificacionRepository.GuardarCambios();

        // Act
        var existe = await _notificacionRepository.ExisteNotificacionHoy(
            TestObjects.ProfesionalId, TipoNotificacion.TurnosSinAsistencia);

        // Assert
        existe.Should().BeTrue();
    }

    [Fact]
    public async Task ExisteNotificacionHoy_ConNotificacionVieja_DevuelveFalse()
    {
        // Arrange
        _notificacionRepository.Crear(new Notificacion
        {
            ProfesionalId = TestObjects.ProfesionalId,
            Tipo = TipoNotificacion.TurnosSinAsistencia,
            Titulo = "Turnos pendientes de asistencia",
            Mensaje = "Tenés 3 turnos pendientes de registrar asistencia.",
            FechaCreacion = DateTime.UtcNow.AddDays(-3)
        });
        await _notificacionRepository.GuardarCambios();

        // Act
        var existe = await _notificacionRepository.ExisteNotificacionHoy(
            TestObjects.ProfesionalId, TipoNotificacion.TurnosSinAsistencia);

        // Assert
        existe.Should().BeFalse();
    }

    [Fact]
    public async Task ExisteNotificacionHoy_ConNotificacionDeHoyDeOtroTipo_DevuelveFalse()
    {
        // Arrange
        _notificacionRepository.Crear(new Notificacion
        {
            ProfesionalId = TestObjects.ProfesionalId,
            Tipo = TipoNotificacion.ErrorRecordatorio,
            Titulo = "Error de recordatorio",
            Mensaje = "No se pudo enviar un recordatorio.",
            FechaCreacion = DateTime.UtcNow
        });
        await _notificacionRepository.GuardarCambios();

        // Act
        var existe = await _notificacionRepository.ExisteNotificacionHoy(
            TestObjects.ProfesionalId, TipoNotificacion.TurnosSinAsistencia);

        // Assert
        existe.Should().BeFalse();
    }
}
