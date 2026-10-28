using AutoMapper;
using Biblioteca.Entities;
using NSubstitute;
using Repository.Pacientes;
using Repository.Profesionales;
using Repository.Turnos;
using Service.Tests.TestSupport;
using Service.Turnos;
using Utils.DTOs.Turno;
using Utils.Exceptions;

namespace Service.Tests.Turnos;

public class TurnoServiceTests
{
    private readonly ITurnoRepository _turnoRepository = Substitute.For<ITurnoRepository>();
    private readonly IPacienteRepository _pacienteRepository = Substitute.For<IPacienteRepository>();
    private readonly IProfesionalRepository _profesionalRepository = Substitute.For<IProfesionalRepository>();
    private readonly IMapper _mapper = MapperStub.Create();

    private TurnoService CrearServicio() =>
        new(_turnoRepository, _pacienteRepository, _profesionalRepository, _mapper);

    private void PacienteYProfesionalValidos(bool vinculado = true)
    {
        _pacienteRepository.GetByIdAsync(TestObjects.PacienteId).Returns(TestObjects.Paciente());
        _profesionalRepository.GetByIdAsync(TestObjects.ProfesionalId).Returns(TestObjects.Profesional());
        _pacienteRepository.IsVinculadoAsync(TestObjects.PacienteId, TestObjects.ProfesionalId).Returns(vinculado);
    }

    private static CrearTurnoDto DtoSueltoValido() => new()
    {
        PacienteId = TestObjects.PacienteId,
        ProfesionalId = TestObjects.ProfesionalId,
        FechaHora = DateTime.UtcNow.AddDays(1),
        DuracionMin = 30
    };

    // ----- CambiarEstado -----

    [Fact]
    public async Task CambiarEstado_TransicionPermitida_ActualizaElEstadoYPersiste()
    {
        // Arrange
        var turno = TestObjects.Turno(id: 5, estado: "Pendiente");
        _turnoRepository.ObtenerPorId(5).Returns(turno);

        // Act
        await CrearServicio().CambiarEstado(5, EstadoTurno.Cancelado);

        // Assert
        turno.Estado.Should().Be("Cancelado");
        await _turnoRepository.Received(1).Actualizar(turno);
        await _turnoRepository.Received(1).GuardarCambios();
    }

    [Fact]
    public async Task CambiarEstado_TurnoInexistente_LanzaNotFoundError()
    {
        // Arrange
        _turnoRepository.ObtenerPorId(99).Returns((Turno?)null);

        // Act
        var accion = () => CrearServicio().CambiarEstado(99, EstadoTurno.Cancelado);

        // Assert
        await accion.Should().ThrowAsync<NotFoundError>();
    }

    [Theory]
    [InlineData("Pendiente", EstadoTurno.Presente)]
    [InlineData("Cancelado", EstadoTurno.Reprogramado)]
    [InlineData("Presente", EstadoTurno.Cancelado)]
    [InlineData("Ausente", EstadoTurno.Reprogramado)]
    public async Task CambiarEstado_TransicionNoPermitida_LanzaConflictError(string estadoActual, EstadoTurno nuevo)
    {
        // Arrange
        var turno = TestObjects.Turno(id: 5, estado: estadoActual);
        _turnoRepository.ObtenerPorId(5).Returns(turno);

        // Act
        var accion = () => CrearServicio().CambiarEstado(5, nuevo);

        // Assert
        await accion.Should().ThrowAsync<ConflictError>();
        await _turnoRepository.DidNotReceive().Actualizar(Arg.Any<Turno>());
    }

    [Fact]
    public async Task CambiarEstado_EstadoDesconocidoEnLaBase_LanzaConflictError()
    {
        // Arrange
        var turno = TestObjects.Turno(id: 5, estado: "EstadoInventado");
        _turnoRepository.ObtenerPorId(5).Returns(turno);

        // Act
        var accion = () => CrearServicio().CambiarEstado(5, EstadoTurno.Cancelado);

        // Assert
        await accion.Should().ThrowAsync<ConflictError>();
    }

    // ----- RegistrarAsistencia -----

    [Fact]
    public async Task RegistrarAsistencia_Asistio_MarcaPresenteYFacturable()
    {
        // Arrange
        var turno = TestObjects.Turno(id: 5, estado: "Pendiente");
        _turnoRepository.ObtenerPorId(5).Returns(turno);
        var antes = DateTime.UtcNow;

        // Act
        await CrearServicio().RegistrarAsistencia(5, asistio: true, justificada: null, observaciones: null);

        // Assert
        turno.Estado.Should().Be("Presente");
        turno.Facturable.Should().BeTrue();
        turno.Justificada.Should().BeFalse();
        turno.FechaRegistroAsistencia.Should().NotBeNull();
        turno.FechaRegistroAsistencia!.Value.Should().BeOnOrAfter(antes);
        await _turnoRepository.Received(1).GuardarCambios();
    }

    [Fact]
    public async Task RegistrarAsistencia_AusenciaJustificada_MarcaAusenteYFacturable()
    {
        // Arrange
        var turno = TestObjects.Turno(id: 5, estado: "Pendiente");
        _turnoRepository.ObtenerPorId(5).Returns(turno);

        // Act
        await CrearServicio().RegistrarAsistencia(5, asistio: false, justificada: true, observaciones: null);

        // Assert
        turno.Estado.Should().Be("Ausente");
        turno.Justificada.Should().BeTrue();
        turno.Facturable.Should().BeTrue();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(null)]
    public async Task RegistrarAsistencia_AusenciaSinJustificar_NoEsFacturable(bool? justificada)
    {
        // Arrange
        var turno = TestObjects.Turno(id: 5, estado: "Pendiente");
        _turnoRepository.ObtenerPorId(5).Returns(turno);

        // Act
        await CrearServicio().RegistrarAsistencia(5, asistio: false, justificada: justificada, observaciones: null);

        // Assert
        turno.Estado.Should().Be("Ausente");
        turno.Justificada.Should().BeFalse();
        turno.Facturable.Should().BeFalse();
    }

    [Fact]
    public async Task RegistrarAsistencia_ConObservaciones_LasGuardaEnElTurno()
    {
        // Arrange
        var turno = TestObjects.Turno(id: 5, estado: "Pendiente");
        _turnoRepository.ObtenerPorId(5).Returns(turno);

        // Act
        await CrearServicio().RegistrarAsistencia(5, asistio: false, justificada: true, observaciones: "Avisó con antelación");

        // Assert
        turno.Observaciones.Should().Be("Avisó con antelación");
    }

    [Theory]
    [InlineData("Presente")]
    [InlineData("Ausente")]
    public async Task RegistrarAsistencia_YaExisteUnaAsistencia_LanzaConflictError(string estado)
    {
        // Arrange
        var turno = TestObjects.Turno(id: 5, estado: estado);
        _turnoRepository.ObtenerPorId(5).Returns(turno);

        // Act
        var accion = () => CrearServicio().RegistrarAsistencia(5, asistio: true, justificada: null, observaciones: null);

        // Assert
        await accion.Should().ThrowAsync<ConflictError>();
    }

    [Fact]
    public async Task RegistrarAsistencia_TurnoInexistente_LanzaNotFoundError()
    {
        // Arrange
        _turnoRepository.ObtenerPorId(99).Returns((Turno?)null);

        // Act
        var accion = () => CrearServicio().RegistrarAsistencia(99, asistio: true, justificada: null, observaciones: null);

        // Assert
        await accion.Should().ThrowAsync<NotFoundError>();
    }

    // ----- ActualizarAsistencia -----

    [Fact]
    public async Task ActualizarAsistencia_TurnoSinAsistenciaRegistrada_LanzaNotFoundError()
    {
        // Arrange
        var turno = TestObjects.Turno(id: 5, estado: "Pendiente");
        _turnoRepository.ObtenerPorId(5).Returns(turno);

        // Act
        var accion = () => CrearServicio().ActualizarAsistencia(5, new ActualizarAsistenciaDto { Asistio = true });

        // Assert
        await accion.Should().ThrowAsync<NotFoundError>();
    }

    [Fact]
    public async Task ActualizarAsistencia_PasaDePresenteAAusenciaInjustificada_RecalculaTrazabilidad()
    {
        // Arrange
        var turno = TestObjects.Turno(id: 5, estado: "Presente");
        turno.Facturable = true;
        _turnoRepository.ObtenerPorId(5).Returns(turno);

        // Act
        await CrearServicio().ActualizarAsistencia(5, new ActualizarAsistenciaDto
        {
            Asistio = false,
            Justificada = false,
            Observaciones = "Corrección administrativa"
        });

        // Assert
        turno.Estado.Should().Be("Ausente");
        turno.Justificada.Should().BeFalse();
        turno.Facturable.Should().BeFalse();
        turno.Observaciones.Should().Be("Corrección administrativa");
        await _turnoRepository.Received(1).GuardarCambios();
    }

    [Fact]
    public async Task ActualizarAsistencia_AusenciaJustificada_SeMantieneFacturable()
    {
        // Arrange
        var turno = TestObjects.Turno(id: 5, estado: "Ausente");
        _turnoRepository.ObtenerPorId(5).Returns(turno);

        // Act
        await CrearServicio().ActualizarAsistencia(5, new ActualizarAsistenciaDto
        {
            Asistio = false,
            Justificada = true
        });

        // Assert
        turno.Estado.Should().Be("Ausente");
        turno.Justificada.Should().BeTrue();
        turno.Facturable.Should().BeTrue();
    }

    // ----- Reprogramar -----

    [Fact]
    public async Task Reprogramar_TurnoPendiente_MueveLaFechaYMarcaReprogramado()
    {
        // Arrange
        var turno = TestObjects.Turno(id: 5, estado: "Pendiente");
        _turnoRepository.ObtenerPorId(5).Returns(turno);
        var nuevaFecha = DateTime.UtcNow.AddDays(2);

        // Act
        await CrearServicio().Reprogramar(5, nuevaFecha);

        // Assert
        turno.FechaHora.Should().Be(nuevaFecha);
        turno.Estado.Should().Be("Reprogramado");
        await _turnoRepository.Received(1).GuardarCambios();
    }

    [Theory]
    [InlineData("Cancelado")]
    [InlineData("Presente")]
    [InlineData("Ausente")]
    public async Task Reprogramar_TurnoEnEstadoNoReprogramable_LanzaConflictError(string estado)
    {
        // Arrange
        var turno = TestObjects.Turno(id: 5, estado: estado);
        _turnoRepository.ObtenerPorId(5).Returns(turno);

        // Act
        var accion = () => CrearServicio().Reprogramar(5, DateTime.UtcNow.AddDays(2));

        // Assert
        await accion.Should().ThrowAsync<ConflictError>();
    }

    [Fact]
    public async Task Reprogramar_ElNuevoHorarioSeSolapaConOtroTurno_LanzaConflictError()
    {
        // Arrange
        var turno = TestObjects.Turno(id: 5, estado: "Pendiente");
        _turnoRepository.ObtenerPorId(5).Returns(turno);
        _turnoRepository.ExisteSolapamiento(
                turno.ProfesionalId, Arg.Any<DateTime>(), turno.DuracionMin, turno.Id)
            .Returns(true);

        // Act
        var accion = () => CrearServicio().Reprogramar(5, DateTime.UtcNow.AddDays(2));

        // Assert
        await accion.Should().ThrowAsync<ConflictError>();
        turno.Estado.Should().Be("Pendiente");
    }

    [Fact]
    public async Task Reprogramar_TurnoInexistente_LanzaNotFoundError()
    {
        // Arrange
        _turnoRepository.ObtenerPorId(99).Returns((Turno?)null);

        // Act
        var accion = () => CrearServicio().Reprogramar(99, DateTime.UtcNow.AddDays(2));

        // Assert
        await accion.Should().ThrowAsync<NotFoundError>();
    }

    // ----- CrearSuelto -----

    [Fact]
    public async Task CrearSuelto_DatosValidos_CreaElTurnoConFechaDeRegistro()
    {
        // Arrange
        PacienteYProfesionalValidos();
        _turnoRepository.ExisteSolapamiento(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<int?>())
            .Returns(false);
        var antes = DateTime.UtcNow;
        Turno? creado = null;
        await _turnoRepository.Agregar(Arg.Do<Turno>(t => creado = t));

        // Act
        var resultado = await CrearServicio().CrearSuelto(DtoSueltoValido());

        // Assert
        creado.Should().NotBeNull();
        creado!.FechaRegistro.Should().BeOnOrAfter(antes);
        creado.Estado.Should().Be("Pendiente");
        resultado.Should().NotBeNull();
        await _turnoRepository.Received(1).GuardarCambios();
    }

    [Fact]
    public async Task CrearSuelto_FechaEnElPasado_LanzaValidationError()
    {
        // Arrange
        var dto = DtoSueltoValido();
        dto.FechaHora = new DateTime(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var accion = () => CrearServicio().CrearSuelto(dto);

        // Assert
        await accion.Should().ThrowAsync<ValidationError>();
    }

    [Fact]
    public async Task CrearSuelto_PacienteInexistente_LanzaValidationError()
    {
        // Arrange
        _pacienteRepository.GetByIdAsync(TestObjects.PacienteId).Returns((Paciente?)null);

        // Act
        var accion = () => CrearServicio().CrearSuelto(DtoSueltoValido());

        // Assert
        await accion.Should().ThrowAsync<ValidationError>();
    }

    [Fact]
    public async Task CrearSuelto_ProfesionalInexistente_LanzaValidationError()
    {
        // Arrange
        _pacienteRepository.GetByIdAsync(TestObjects.PacienteId).Returns(TestObjects.Paciente());
        _profesionalRepository.GetByIdAsync(TestObjects.ProfesionalId).Returns((Profesional?)null);

        // Act
        var accion = () => CrearServicio().CrearSuelto(DtoSueltoValido());

        // Assert
        await accion.Should().ThrowAsync<ValidationError>();
    }

    [Fact]
    public async Task CrearSuelto_PacienteNoVinculado_LanzaConflictError()
    {
        // Arrange
        PacienteYProfesionalValidos(vinculado: false);

        // Act
        var accion = () => CrearServicio().CrearSuelto(DtoSueltoValido());

        // Assert
        await accion.Should().ThrowAsync<ConflictError>();
    }

    [Fact]
    public async Task CrearSuelto_HorarioOcupado_LanzaConflictError()
    {
        // Arrange
        PacienteYProfesionalValidos();
        _turnoRepository.ExisteSolapamiento(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<int?>())
            .Returns(true);

        // Act
        var accion = () => CrearServicio().CrearSuelto(DtoSueltoValido());

        // Assert
        await accion.Should().ThrowAsync<ConflictError>();
        await _turnoRepository.DidNotReceive().Agregar(Arg.Any<Turno>());
    }

    // ----- ObtenerAgenda -----

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 1001)]
    public async Task ObtenerAgenda_PaginacionInvalida_LanzaValidationError(int page, int pageSize)
    {
        // Act
        var accion = () => CrearServicio().ObtenerAgenda(DateTime.UtcNow, DateTime.UtcNow, page, pageSize, null);

        // Assert
        await accion.Should().ThrowAsync<ValidationError>();
    }

    [Fact]
    public async Task ObtenerAgenda_ConResultados_CalculaElTotalDePaginas()
    {
        // Arrange
        _turnoRepository.ObtenerPorRangoFecha(Arg.Any<DateTime>(), Arg.Any<DateTime>(), 2, 10, null)
            .Returns((Items: Enumerable.Range(1, 10).Select(i => TestObjects.Turno(i)), TotalItems: 25));

        // Act
        var resultado = await CrearServicio().ObtenerAgenda(DateTime.UtcNow, DateTime.UtcNow, 2, 10, null);

        // Assert
        resultado.TotalItems.Should().Be(25);
        resultado.TotalPages.Should().Be(3);
        resultado.Page.Should().Be(2);
        resultado.Items.Should().HaveCount(10);
    }

    // ----- ObtenerPorId / ObtenerResumen -----

    [Fact]
    public async Task ObtenerPorId_TurnoInexistente_LanzaNotFoundError()
    {
        // Arrange
        _turnoRepository.ObtenerPorId(99).Returns((Turno?)null);

        // Act
        var accion = () => CrearServicio().ObtenerPorId(99);

        // Assert
        await accion.Should().ThrowAsync<NotFoundError>();
    }

    [Fact]
    public async Task ObtenerPorId_TurnoExistente_DevuelveTurnoDto()
    {
        // Arrange
        var turno = TestObjects.Turno(5, "Pendiente");
        _turnoRepository.ObtenerPorId(5).Returns(turno);

        // Act
        var resultado = await CrearServicio().ObtenerPorId(5);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(5);
        resultado.Estado.Should().Be("Pendiente");
    }

    [Fact]
    public async Task ObtenerFacturables_PacienteConTurnos_DevuelveListaDeTurnosFacturables()
    {
        // Arrange
        var turnos = new List<Turno>
        {
            TestObjects.Turno(1, "Presente"),
            TestObjects.Turno(2, "Ausente")
        };
        var desde = DateTime.UtcNow.AddDays(-7);
        var hasta = DateTime.UtcNow;
        _turnoRepository.ObtenerFacturables(TestObjects.PacienteId, desde, hasta).Returns(turnos);

        // Act
        var resultado = await CrearServicio().ObtenerFacturables(TestObjects.PacienteId, desde, hasta);

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Select(r => r.Id).Should().Equal(1, 2);
    }

    [Fact]
    public async Task ObtenerHistorialPorTurnoFijo_ReglaConTurnos_DevuelveHistorialCompleto()
    {
        // Arrange
        var turnos = new List<Turno>
        {
            TestObjects.Turno(1, "Presente"),
            TestObjects.Turno(2, "Cancelado")
        };
        _turnoRepository.ObtenerTodosPorTurnoFijo(TestObjects.TurnoFijoId).Returns(turnos);

        // Act
        var resultado = await CrearServicio().ObtenerHistorialPorTurnoFijo(TestObjects.TurnoFijoId);

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Select(r => r.Id).Should().Equal(1, 2);
    }

    [Fact]
    public async Task ObtenerResumenPorTurnoFijo_CalculaLosTotalesPorEstado()
    {
        // Arrange
        var justificada = TestObjects.Turno(3, "Ausente");
        justificada.Justificada = true;
        var turnos = new List<Turno>
        {
            TestObjects.Turno(1, "Presente"),
            TestObjects.Turno(2, "Presente"),
            justificada,
            TestObjects.Turno(4, "Ausente"),
            TestObjects.Turno(5, "Pendiente")
        };
        _turnoRepository.ObtenerFuturosPorTurnoFijo(TestObjects.TurnoFijoId, Arg.Any<DateTime>()).Returns(turnos);

        // Act
        var resumen = await CrearServicio().ObtenerResumenPorTurnoFijo(TestObjects.TurnoFijoId);

        // Assert
        resumen.TotalTurnos.Should().Be(5);
        resumen.TotalAsistencias.Should().Be(2);
        resumen.TotalAusenciasJustificadas.Should().Be(1);
        resumen.TotalAusenciasInjustificadas.Should().Be(1);
    }
}
