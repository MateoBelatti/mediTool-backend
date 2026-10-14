using Biblioteca.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Repository.Turnos;
using Repository.TurnosFijos;
using Service.Tests.TestSupport;
using Service.Turnos;
using Utils.Exceptions;
using Utils.Helpers;

namespace Service.Tests.Turnos;

public class GenerarInstanciasTurnoServiceTests
{
    // 2035-01-01 es lunes; se usa una fecha lejana para no depender del reloj de ejecución.
    private static readonly DateOnly Lunes = new(2035, 1, 1);
    private const int LunesInt = 1;

    private readonly ITurnoFijoRepository _turnoFijoRepository = Substitute.For<ITurnoFijoRepository>();
    private readonly ITurnoRepository _turnoRepository = Substitute.For<ITurnoRepository>();

    private GenerarInstanciasTurnoService CrearServicio(ITurnoRepository? turnoRepository = null) =>
        new(
            _turnoFijoRepository,
            turnoRepository ?? _turnoRepository,
            NullLogger<GenerarInstanciasTurnoService>.Instance);

    private void SinTurnosPrevios(bool solapa = false)
    {
        _turnoRepository.ObtenerUltimoPorTurnoFijo(Arg.Any<int>()).Returns((Turno?)null);
        _turnoRepository.ExisteSolapamiento(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<int?>())
            .Returns(solapa);
    }

    [Fact]
    public async Task GenerarInstancias_ReglaSemanal_GeneraUnTurnoPorCadaOcurrenciaEnElRango()
    {
        // Arrange
        var regla = TestObjects.TurnoFijo(diaSemana: LunesInt, fechaInicio: Lunes);
        var hasta = new DateTime(2035, 1, 31, 0, 0, 0, DateTimeKind.Unspecified);
        SinTurnosPrevios();

        var generados = new List<Turno>();
        await _turnoRepository.Agregar(Arg.Do<Turno>(generados.Add));

        // Act
        var cantidad = await CrearServicio().GenerarInstancias(regla, hasta);

        // Assert: 1, 8, 15, 22 y 29 de enero (5 lunes)
        cantidad.Should().Be(5);
        generados.Should().HaveCount(5);
        generados.Should().OnlyContain(t => t.Estado == "Pendiente");
        generados.Should().OnlyContain(t => t.TurnoFijoId == regla.Id);
        generados.Should().OnlyContain(t => t.DuracionMin == regla.DuracionMin);
        generados.Should().OnlyContain(t => DateTimeHelper.UtcALocal(t.FechaHora).DayOfWeek == DayOfWeek.Monday);

        var fechas = generados.Select(t => DateOnly.FromDateTime(DateTimeHelper.UtcALocal(t.FechaHora))).ToList();
        fechas.Should().Equal(
            Lunes, Lunes.AddDays(7), Lunes.AddDays(14), Lunes.AddDays(21), Lunes.AddDays(28));
    }

    [Fact]
    public async Task GenerarInstancias_ReglaSemanal_ConvierteLaHoraLocalArgentinaAUtc()
    {
        // Arrange
        var regla = TestObjects.TurnoFijo(diaSemana: LunesInt, hora: new TimeSpan(10, 0, 0), fechaInicio: Lunes);
        SinTurnosPrevios();

        var generados = new List<Turno>();
        await _turnoRepository.Agregar(Arg.Do<Turno>(generados.Add));

        // Act
        await CrearServicio().GenerarInstancias(regla, Lunes.ToDateTime(new TimeOnly(0, 0)).AddDays(1));

        // Assert: America/Argentina/Buenos_Aires es UTC-3 (sin horario de verano)
        var turno = generados.Should().ContainSingle().Subject;
        DateTimeHelper.UtcALocal(turno.FechaHora).Hour.Should().Be(10);
        turno.FechaHora.Hour.Should().Be(13);
        turno.FechaHora.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task GenerarInstancias_ConFechaFin_NoGeneraMasAllaDelLimiteDeLaRegla()
    {
        // Arrange
        var regla = TestObjects.TurnoFijo(
            diaSemana: LunesInt,
            fechaInicio: Lunes,
            fechaFin: new DateOnly(2035, 1, 15));
        SinTurnosPrevios();

        var generados = new List<Turno>();
        await _turnoRepository.Agregar(Arg.Do<Turno>(generados.Add));

        // Act: el horizonte es mucho mayor que FechaFin
        var cantidad = await CrearServicio().GenerarInstancias(regla, new DateTime(2036, 1, 1));

        // Assert: sólo 1, 8 y 15 de enero
        cantidad.Should().Be(3);
        generados.Should().HaveCount(3);
    }

    [Fact]
    public async Task GenerarInstancias_FechaInicioNoCoincideConElDiaConfigurado_AvanzaHastaElSiguienteDiaSemana()
    {
        // Arrange: 2035-01-02 es martes, pero la regla es lunes
        var regla = TestObjects.TurnoFijo(diaSemana: LunesInt, fechaInicio: new DateOnly(2035, 1, 2));
        SinTurnosPrevios();
        var hasta = new DateTime(2035, 1, 31);

        var generados = new List<Turno>();
        await _turnoRepository.Agregar(Arg.Do<Turno>(generados.Add));

        // Act
        await CrearServicio().GenerarInstancias(regla, hasta);

        // Assert: el primer turno cae el lunes 8 de enero
        DateOnly.FromDateTime(DateTimeHelper.UtcALocal(generados.First().FechaHora))
            .Should().Be(new DateOnly(2035, 1, 8));
    }

    [Fact]
    public async Task GenerarInstancias_TurnosYaMaterializados_NoDuplicaLasInstanciasExistentes()
    {
        // Arrange
        var regla = TestObjects.TurnoFijo(diaSemana: LunesInt, fechaInicio: Lunes);
        var hasta = new DateTime(2035, 1, 31);
        var repositorio = new FakeTurnoRepository();
        var servicio = CrearServicio(repositorio);

        // Act
        var primeraCorrida = await servicio.GenerarInstancias(regla, hasta);
        var segundaCorrida = await servicio.GenerarInstancias(regla, hasta);

        // Assert: la primera materializa 5 turnos y la segunda no agrega ninguno
        primeraCorrida.Should().Be(5);
        segundaCorrida.Should().Be(0);
        repositorio.Turnos.Should().HaveCount(5);
        repositorio.Turnos.Select(t => t.FechaHora).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task GenerarInstancias_HayOcurrenciasEnElPasado_LasSaltaYGeneraSoloFuturas()
    {
        // Arrange
        var ahora = DateTime.UtcNow;
        var origen = DateOnly.FromDateTime(ahora.AddYears(-1));
        var regla = TestObjects.TurnoFijo(diaSemana: (int)origen.DayOfWeek, fechaInicio: origen);
        SinTurnosPrevios();
        var hasta = ahora.AddDays(21);

        var generados = new List<Turno>();
        await _turnoRepository.Agregar(Arg.Do<Turno>(generados.Add));

        // Act
        var cantidad = await CrearServicio().GenerarInstancias(regla, hasta);

        // Assert
        cantidad.Should().BeGreaterThan(0);
        generados.Should().OnlyContain(t => t.FechaHora >= ahora.AddMinutes(-1));
    }

    [Fact]
    public async Task GenerarInstancias_LaReglaNoProduceOcurrenciasEnElHorizonte_DevuelveCero()
    {
        // Arrange: la regla empieza después del horizonte solicitado
        var regla = TestObjects.TurnoFijo(diaSemana: LunesInt, fechaInicio: new DateOnly(2035, 6, 1));
        SinTurnosPrevios();

        // Act
        var cantidad = await CrearServicio().GenerarInstancias(regla, new DateTime(2035, 1, 31));

        // Assert
        cantidad.Should().Be(0);
        await _turnoRepository.DidNotReceive().Agregar(Arg.Any<Turno>());
    }

    [Fact]
    public async Task GenerarInstancias_SolapaConUnTurnoExistenteDelProfesional_LanzaConflictError()
    {
        // Arrange
        var regla = TestObjects.TurnoFijo(diaSemana: LunesInt, fechaInicio: Lunes);
        SinTurnosPrevios(solapa: true);

        // Act
        var accion = () => CrearServicio().GenerarInstancias(regla, new DateTime(2035, 1, 31));

        // Assert
        await accion.Should().ThrowAsync<ConflictError>();
        await _turnoRepository.DidNotReceive().Agregar(Arg.Any<Turno>());
    }

    [Fact]
    public async Task GenerarParaTodasLasReglasActivas_UnaReglaFalla_ContinuaConLasDemas()
    {
        // Arrange: la regla 1 falla por solapamiento, la regla 2 genera correctamente
        var reglaQueFalla = TestObjects.TurnoFijo(id: 100, profesionalId: 1);
        var reglaQueGenera = TestObjects.TurnoFijo(id: 200, profesionalId: 2);
        _turnoFijoRepository.ObtenerActivos().Returns(new List<TurnoFijo> { reglaQueFalla, reglaQueGenera });
        _turnoRepository.ObtenerUltimoPorTurnoFijo(Arg.Any<int>()).Returns((Turno?)null);
        _turnoRepository.ExisteSolapamiento(1, Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<int?>()).Returns(true);
        _turnoRepository.ExisteSolapamiento(2, Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<int?>()).Returns(false);

        var generados = new List<Turno>();
        await _turnoRepository.Agregar(Arg.Do<Turno>(generados.Add));

        // Act
        var accion = () => CrearServicio().GenerarParaTodasLasReglasActivas(new DateTime(2035, 1, 31));

        // Assert
        await accion.Should().NotThrowAsync();
        generados.Should().NotBeEmpty();
        generados.Should().OnlyContain(t => t.ProfesionalId == 2);
    }

    [Fact]
    public async Task GenerarParaTodasLasReglasActivas_EjecutadoMultiplesVeces_EsIdempotenteYNoDuplicaTurnos()
    {
        // Arrange
        var regla1 = TestObjects.TurnoFijo(id: 10, profesionalId: 1, diaSemana: LunesInt, fechaInicio: Lunes);
        var regla2 = TestObjects.TurnoFijo(id: 20, profesionalId: 2, diaSemana: LunesInt, fechaInicio: Lunes);
        _turnoFijoRepository.ObtenerActivos().Returns(new List<TurnoFijo> { regla1, regla2 });

        var fakeRepo = new FakeTurnoRepository();
        var servicio = CrearServicio(fakeRepo);
        var hasta = new DateTime(2035, 1, 31);

        // Act: ejecutar dos veces consecutivas el proceso batch (job)
        await servicio.GenerarParaTodasLasReglasActivas(hasta);
        var totalPrimeraCorrida = fakeRepo.Turnos.Count;

        await servicio.GenerarParaTodasLasReglasActivas(hasta);
        var totalSegundaCorrida = fakeRepo.Turnos.Count;

        // Assert: 5 turnos por cada regla = 10 turnos en total, y la segunda corrida no agrega nada nuevo
        totalPrimeraCorrida.Should().Be(10);
        totalSegundaCorrida.Should().Be(10);
        fakeRepo.Turnos.Select(t => (t.TurnoFijoId, t.FechaHora)).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task CancelarInstanciasFuturas_MarcaTodosLosTurnosFuturosComoCancelados()
    {
        // Arrange
        var futuros = new List<Turno>
        {
            TestObjects.Turno(id: 1, estado: "Pendiente"),
            TestObjects.Turno(id: 2, estado: "Reprogramado"),
            TestObjects.Turno(id: 3, estado: "Presente")
        };
        _turnoRepository.ObtenerFuturosPorTurnoFijo(TestObjects.TurnoFijoId, Arg.Any<DateTime>()).Returns(futuros);

        // Act
        await CrearServicio().CancelarInstanciasFuturas(TestObjects.TurnoFijoId);

        // Assert
        futuros.Should().OnlyContain(t => t.Estado == "Cancelado");
        await _turnoRepository.Received(1).ActualizarRango(futuros);
        await _turnoRepository.Received(1).GuardarCambios();
    }

    [Fact]
    public async Task ActualizarInstanciasFuturas_ReglaEditada_ActualizaSoloLosTurnosPendientes()
    {
        // Arrange
        var regla = TestObjects.TurnoFijo(diaSemana: LunesInt, hora: new TimeSpan(11, 0, 0), duracionMin: 45);
        var pendiente = TestObjects.Turno(id: 1, estado: "Pendiente");
        var cancelado = TestObjects.Turno(id: 2, estado: "Cancelado");
        _turnoRepository.ObtenerFuturosPorTurnoFijo(regla.Id, Arg.Any<DateTime>())
            .Returns(new List<Turno> { pendiente, cancelado });

        // Act
        await CrearServicio().ActualizarInstanciasFuturas(regla);

        // Assert
        pendiente.DuracionMin.Should().Be(45);
        DateTimeHelper.UtcALocal(pendiente.FechaHora).Hour.Should().Be(11);
        cancelado.DuracionMin.Should().Be(30);
        await _turnoRepository.Received(1).ActualizarRango(Arg.Any<List<Turno>>());
    }

    [Fact]
    public async Task ActualizarInstanciasFuturas_LaNuevaFechaCaeEnElPasado_CancelaElTurno()
    {
        // Arrange: turno futuro cuya reprogramación resultaría en una fecha pasada
        var ahora = DateTime.UtcNow;
        var regla = TestObjects.TurnoFijo(
            diaSemana: (int)DateOnly.FromDateTime(ahora.AddDays(5)).DayOfWeek,
            hora: new TimeSpan(0, 0, 0),
            duracionMin: 5);
        var turno = TestObjects.Turno(id: 1, estado: "Pendiente", fechaHora: ahora.AddMinutes(2));
        _turnoRepository.ObtenerFuturosPorTurnoFijo(regla.Id, Arg.Any<DateTime>())
            .Returns(new List<Turno> { turno });

        // Act
        await CrearServicio().ActualizarInstanciasFuturas(regla);

        // Assert
        turno.Estado.Should().Be("Cancelado");
    }
}
