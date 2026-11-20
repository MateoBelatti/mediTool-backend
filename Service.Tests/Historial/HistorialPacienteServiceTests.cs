using AutoMapper;
using Biblioteca.Entities;
using FluentAssertions;
using NSubstitute;
using Repository.Informes;
using Repository.Pacientes;
using Repository.Turnos;
using Service.Pacientes;
using Service.Tests.TestSupport;
using Utils.Exceptions;
using Xunit;

namespace Service.Tests.Historial;

public class HistorialPacienteServiceTests
{
    private readonly IPacienteRepository _pacienteRepository = Substitute.For<IPacienteRepository>();
    private readonly ITurnoRepository _turnoRepository = Substitute.For<ITurnoRepository>();
    private readonly IInformeRepository _informeRepository = Substitute.For<IInformeRepository>();
    private readonly IMapper _mapper = MapperStub.Create();

    private HistorialPacienteService CrearServicio() =>
        new(_pacienteRepository, _turnoRepository, _informeRepository, _mapper);

    private void SetupPaciente()
    {
        _pacienteRepository.GetByIdAsync(TestObjects.PacienteId)
            .Returns(TestObjects.Paciente());
    }

    private void SetupTurnos(params Turno[] turnos)
    {
        _turnoRepository.ObtenerTodosPorPaciente(TestObjects.PacienteId)
            .Returns(turnos.ToList());
    }

    private void SetupInformes(params Informe[] informes)
    {
        _informeRepository.GetByPacienteIdAsync(TestObjects.PacienteId)
            .Returns(Task.FromResult<IEnumerable<Informe>>(informes));
    }

    private static Turno TurnoConProfesional(
        int id,
        DateTime fechaHora,
        string estado = "Pendiente",
        Informe? informe = null)
    {
        var turno = TestObjects.Turno(id: id, estado: estado, fechaHora: fechaHora);
        turno.Profesional = TestObjects.Profesional();
        turno.Informe = informe;

        if (informe != null)
        {
            turno.InformeId = informe.Id;
            informe.Turno = turno;
        }

        return turno;
    }

    private static Informe InformeSuelto(int id, DateOnly fecha)
    {
        var informe = TestObjects.Informe(id: id, fecha: fecha);
        informe.Profesional = TestObjects.Profesional();
        return informe;
    }

    [Fact]
    public async Task GetHistorialAsync_MezclaTurnosEInformesSueltos_LosOrdenaDescendente()
    {
        // Arrange
        SetupPaciente();
        var informeSuelto = InformeSuelto(20, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)));
        var turno = TurnoConProfesional(1, DateTime.UtcNow.AddDays(-2));
        SetupTurnos(turno);
        SetupInformes(informeSuelto);

        // Act
        var result = await CrearServicio().GetHistorialAsync(TestObjects.PacienteId, 1, 20);

        // Assert
        result.Historial.Items.Should().HaveCount(2);
        result.Historial.Items.First().Tipo.Should().Be("Turno");
        result.Historial.Items.First().Turno!.Id.Should().Be(1);
        result.Historial.Items.Last().Tipo.Should().Be("Informe");
        result.Historial.Items.Last().Informe!.Id.Should().Be(20);
        result.Historial.TotalItems.Should().Be(2);
    }

    [Fact]
    public async Task GetHistorialAsync_IncluyeInformeAnidadoEnSuTurno_SinDuplicarloComoEvento()
    {
        // Arrange
        SetupPaciente();
        var informe = InformeSuelto(7, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)));
        var turno = TurnoConProfesional(1, DateTime.UtcNow.AddDays(-2), informe: informe);
        SetupTurnos(turno);
        SetupInformes(informe);

        // Act
        var result = await CrearServicio().GetHistorialAsync(TestObjects.PacienteId, 1, 20);

        // Assert
        result.Historial.Items.Should().HaveCount(1);
        var evento = result.Historial.Items.Single();
        evento.Tipo.Should().Be("Turno");
        evento.Informe.Should().NotBeNull();
        evento.Informe!.Id.Should().Be(7);
        evento.Informe.TurnoId.Should().Be(1);
    }

    [Fact]
    public async Task GetHistorialAsync_TurnosFuturos_QuedanEnProximosYNoEnElHistorial()
    {
        // Arrange
        SetupPaciente();
        var pasado = TurnoConProfesional(1, DateTime.UtcNow.AddDays(-2));
        var futuro = TurnoConProfesional(2, DateTime.UtcNow.AddDays(2));
        SetupTurnos(pasado, futuro);
        SetupInformes();

        // Act
        var result = await CrearServicio().GetHistorialAsync(TestObjects.PacienteId, 1, 20);

        // Assert
        result.Historial.Items.Should().ContainSingle();
        result.Historial.Items.Single().Turno!.Id.Should().Be(1);
        result.ProximosTurnos.Should().ContainSingle();
        result.ProximosTurnos.Single().Turno!.Id.Should().Be(2);
    }

    [Fact]
    public async Task GetHistorialAsync_TurnoFuturoCancelado_NoApareceEnProximos()
    {
        // Arrange
        SetupPaciente();
        var futuroCancelado = TurnoConProfesional(3, DateTime.UtcNow.AddDays(3), estado: "Cancelado");
        SetupTurnos(futuroCancelado);
        SetupInformes();

        // Act
        var result = await CrearServicio().GetHistorialAsync(TestObjects.PacienteId, 1, 20);

        // Assert
        result.Historial.Items.Should().BeEmpty();
        result.ProximosTurnos.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHistorialAsync_SoloConInforme_DevuelveSoloEventosQueTienenInforme()
    {
        // Arrange
        SetupPaciente();
        var informe = InformeSuelto(9, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-4)));
        var conInforme = TurnoConProfesional(1, DateTime.UtcNow.AddDays(-2), informe: informe);
        var sinInforme = TurnoConProfesional(2, DateTime.UtcNow.AddDays(-1));
        SetupTurnos(conInforme, sinInforme);
        SetupInformes(informe);

        // Act
        var result = await CrearServicio().GetHistorialAsync(
            TestObjects.PacienteId,
            1,
            20,
            soloConInforme: true);

        // Assert
        result.Historial.Items.Should().ContainSingle();
        result.Historial.Items.Single().Turno!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetHistorialAsync_FiltroTipoInforme_IncluyeTurnosConInformeEInformesSueltos()
    {
        // Arrange
        SetupPaciente();
        var informeSuelto = InformeSuelto(9, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-4)));
        var informeTurno = InformeSuelto(10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)));
        var conInforme = TurnoConProfesional(1, DateTime.UtcNow.AddDays(-2), informe: informeTurno);
        var sinInforme = TurnoConProfesional(2, DateTime.UtcNow.AddDays(-1));
        SetupTurnos(conInforme, sinInforme);
        SetupInformes(informeSuelto, informeTurno);

        // Act
        var result = await CrearServicio().GetHistorialAsync(
            TestObjects.PacienteId,
            1,
            20,
            tipo: "informe");

        // Assert
        result.Historial.Items.Should().HaveCount(2);
        result.Historial.Items.Should().OnlyContain(e => e.Informe != null);
    }

    [Fact]
    public async Task GetHistorialAsync_FiltroEstado_DevuelveSoloTurnosConEseEstado()
    {
        // Arrange
        SetupPaciente();
        var presente = TurnoConProfesional(1, DateTime.UtcNow.AddDays(-2), estado: "Presente");
        var ausente = TurnoConProfesional(2, DateTime.UtcNow.AddDays(-1), estado: "Ausente");
        SetupTurnos(presente, ausente);
        SetupInformes();

        // Act
        var result = await CrearServicio().GetHistorialAsync(
            TestObjects.PacienteId,
            1,
            20,
            estado: "Presente");

        // Assert
        result.Historial.Items.Should().ContainSingle();
        result.Historial.Items.Single().Turno!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetHistorialAsync_PaginaResultados()
    {
        // Arrange
        SetupPaciente();
        SetupTurnos(
            TurnoConProfesional(1, DateTime.UtcNow.AddDays(-3)),
            TurnoConProfesional(2, DateTime.UtcNow.AddDays(-2)),
            TurnoConProfesional(3, DateTime.UtcNow.AddDays(-1)));
        SetupInformes();

        // Act
        var result = await CrearServicio().GetHistorialAsync(TestObjects.PacienteId, 2, 1);

        // Assert
        result.Historial.Page.Should().Be(2);
        result.Historial.PageSize.Should().Be(1);
        result.Historial.TotalItems.Should().Be(3);
        result.Historial.TotalPages.Should().Be(3);
        result.Historial.Items.Should().ContainSingle();
        result.Historial.Items.Single().Turno!.Id.Should().Be(2);
    }

    [Fact]
    public async Task GetHistorialAsync_PacienteInexistente_LanzaNotFound()
    {
        // Arrange
        _pacienteRepository.GetByIdAsync(TestObjects.PacienteId)
            .Returns((Paciente?)null);

        // Act
        var act = () => CrearServicio().GetHistorialAsync(TestObjects.PacienteId, 1, 20);

        // Assert
        await act.Should().ThrowAsync<NotFoundError>();
    }

    [Fact]
    public async Task GetHistorialAsync_PageSizeInvalido_LanzaValidationError()
    {
        // Act
        var act = () => CrearServicio().GetHistorialAsync(TestObjects.PacienteId, 1, 101);

        // Assert
        await act.Should().ThrowAsync<ValidationError>();
    }
}
