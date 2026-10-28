using AutoMapper;
using Biblioteca.Entities;
using FluentAssertions;
using NSubstitute;
using Repository.Informes;
using Repository.Pacientes;
using Repository.Turnos;
using Service.Informes;
using Service.Tests.TestSupport;
using Utils.DTOs.Informe;
using Utils.Exceptions;
using Xunit;

namespace Service.Tests.Informes;

public class InformeServiceTests
{
    private readonly IInformeRepository _informeRepository = Substitute.For<IInformeRepository>();
    private readonly IPacienteRepository _pacienteRepository = Substitute.For<IPacienteRepository>();
    private readonly ITurnoRepository _turnoRepository = Substitute.For<ITurnoRepository>();
    private readonly IMapper _mapper = MapperStub.Create();

    private InformeService CrearServicio() =>
        new(_informeRepository, _pacienteRepository, _turnoRepository, _mapper);

    private void SetupPacienteVinculado(int pacienteId = TestObjects.PacienteId, int profesionalId = TestObjects.ProfesionalId, bool vinculado = true)
    {
        _pacienteRepository.IsVinculadoAsync(pacienteId, profesionalId).Returns(vinculado);
    }

    private static InformeCreateDto DtoCreacionValido(int? turnoId = null, int? pacienteId = TestObjects.PacienteId) => new()
    {
        PacienteId = pacienteId,
        ProfesionalId = TestObjects.ProfesionalId,
        TurnoId = turnoId,
        Fecha = new DateOnly(2035, 1, 8),
        Tipo = "Evolución",
        Contenido = "Nota de prueba",
        Estado = "Pendiente"
    };

    [Fact]
    public async Task AddAsync_SinTurno_CreaInformeCorrectamente()
    {
        // Arrange
        SetupPacienteVinculado();
        var dto = DtoCreacionValido();
        _informeRepository.AddAsync(Arg.Any<Informe>())
            .Returns(ci => ci.Arg<Informe>());

        // Act
        var result = await CrearServicio().AddAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.TurnoId.Should().BeNull();
        await _informeRepository.Received(1).AddAsync(Arg.Any<Informe>());
        await _informeRepository.Received(1).GuardarCambios();
    }

    [Fact]
    public async Task AddAsync_ConTurnoValidoDelMismoProfesional_CreaInformeConTurnoId()
    {
        // Arrange
        const int turnoId = 15;
        SetupPacienteVinculado();
        var turno = TestObjects.Turno(id: turnoId, pacienteId: TestObjects.PacienteId, profesionalId: TestObjects.ProfesionalId);
        _turnoRepository.ObtenerPorId(turnoId).Returns(turno);
        _informeRepository.ExisteParaTurnoAsync(turnoId).Returns(false);
        _informeRepository.AddAsync(Arg.Any<Informe>())
            .Returns(ci => ci.Arg<Informe>());

        var dto = DtoCreacionValido(turnoId: turnoId);

        // Act
        var result = await CrearServicio().AddAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.TurnoId.Should().Be(turnoId);
        await _informeRepository.Received(1).AddAsync(Arg.Is<Informe>(i => i.TurnoId == turnoId));
        await _informeRepository.Received(1).GuardarCambios();
    }

    [Fact]
    public async Task AddAsync_ConTurnoDeOtroProfesional_LanzaForbiddenError()
    {
        // Arrange
        const int turnoId = 15;
        const int otroProfesionalId = 99;
        var turno = TestObjects.Turno(id: turnoId, pacienteId: TestObjects.PacienteId, profesionalId: otroProfesionalId);
        _turnoRepository.ObtenerPorId(turnoId).Returns(turno);

        var dto = DtoCreacionValido(turnoId: turnoId);

        // Act
        var act = () => CrearServicio().AddAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ForbiddenError>()
            .WithMessage("*dueño del turno*");
        await _informeRepository.DidNotReceive().AddAsync(Arg.Any<Informe>());
    }

    [Fact]
    public async Task AddAsync_ConTurnoInexistente_LanzaNotFoundError()
    {
        // Arrange
        const int turnoId = 999;
        _turnoRepository.ObtenerPorId(turnoId).Returns((Turno?)null);

        var dto = DtoCreacionValido(turnoId: turnoId);

        // Act
        var act = () => CrearServicio().AddAsync(dto);

        // Assert
        await act.Should().ThrowAsync<NotFoundError>()
            .WithMessage($"*Turno {turnoId} no encontrado*");
        await _informeRepository.DidNotReceive().AddAsync(Arg.Any<Informe>());
    }

    [Fact]
    public async Task AddAsync_ConPacienteDiferenteAlDelTurno_LanzaConflictError()
    {
        // Arrange
        const int turnoId = 15;
        const int pacienteTurnoId = 1;
        const int pacienteDtoId = 2;

        var turno = TestObjects.Turno(id: turnoId, pacienteId: pacienteTurnoId, profesionalId: TestObjects.ProfesionalId);
        _turnoRepository.ObtenerPorId(turnoId).Returns(turno);

        var dto = DtoCreacionValido(turnoId: turnoId, pacienteId: pacienteDtoId);

        // Act
        var act = () => CrearServicio().AddAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ConflictError>()
            .WithMessage("*paciente no coincide*");
        await _informeRepository.DidNotReceive().AddAsync(Arg.Any<Informe>());
    }

    [Fact]
    public async Task AddAsync_SinPacienteId_HeredaElPacienteDelTurno()
    {
        // Arrange
        const int turnoId = 15;
        SetupPacienteVinculado(TestObjects.PacienteId, TestObjects.ProfesionalId);
        var turno = TestObjects.Turno(id: turnoId, pacienteId: TestObjects.PacienteId, profesionalId: TestObjects.ProfesionalId);
        _turnoRepository.ObtenerPorId(turnoId).Returns(turno);
        _informeRepository.ExisteParaTurnoAsync(turnoId).Returns(false);
        _informeRepository.AddAsync(Arg.Any<Informe>())
            .Returns(ci => ci.Arg<Informe>());

        var dto = DtoCreacionValido(turnoId: turnoId, pacienteId: null);

        // Act
        var result = await CrearServicio().AddAsync(dto);

        // Assert
        result.PacienteId.Should().Be(TestObjects.PacienteId);
        dto.PacienteId.Should().Be(TestObjects.PacienteId);
    }

    [Fact]
    public async Task AddAsync_TurnoYaTieneInforme_LanzaConflictError()
    {
        // Arrange
        const int turnoId = 15;
        var turno = TestObjects.Turno(id: turnoId, pacienteId: TestObjects.PacienteId, profesionalId: TestObjects.ProfesionalId);
        _turnoRepository.ObtenerPorId(turnoId).Returns(turno);
        _informeRepository.ExisteParaTurnoAsync(turnoId).Returns(true);

        var dto = DtoCreacionValido(turnoId: turnoId);

        // Act
        var act = () => CrearServicio().AddAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ConflictError>()
            .WithMessage("*ya tiene un informe asignado*");
        await _informeRepository.DidNotReceive().AddAsync(Arg.Any<Informe>());
    }

    [Fact]
    public async Task UpdateAsync_ReasignaTurnoDeOtroProfesional_LanzaForbiddenError()
    {
        // Arrange
        const int informeId = 1;
        const int turnoNuevoId = 20;
        const int otroProfesionalId = 99;

        var existing = TestObjects.Informe(id: informeId, turnoId: null, profesionalId: TestObjects.ProfesionalId);
        _informeRepository.GetByIdAsync(informeId).Returns(existing);

        var turnoNuevo = TestObjects.Turno(id: turnoNuevoId, pacienteId: TestObjects.PacienteId, profesionalId: otroProfesionalId);
        _turnoRepository.ObtenerPorId(turnoNuevoId).Returns(turnoNuevo);

        var dto = new InformeUpdateDto
        {
            PacienteId = TestObjects.PacienteId,
            ProfesionalId = TestObjects.ProfesionalId,
            TurnoId = turnoNuevoId,
            Fecha = new DateOnly(2035, 1, 8),
            Tipo = "Evolución",
            Contenido = "Editado",
            Estado = "Completado"
        };

        // Act
        var act = () => CrearServicio().UpdateAsync(informeId, dto);

        // Assert
        await act.Should().ThrowAsync<ForbiddenError>()
            .WithMessage("*mismo profesional*");
        await _informeRepository.DidNotReceive().UpdateAsync(Arg.Any<Informe>());
    }

    [Fact]
    public async Task UpdateAsync_TurnoNuevoYaTieneInforme_LanzaConflictError()
    {
        // Arrange
        const int informeId = 1;
        const int turnoNuevoId = 20;

        var existing = TestObjects.Informe(id: informeId, turnoId: null, profesionalId: TestObjects.ProfesionalId);
        _informeRepository.GetByIdAsync(informeId).Returns(existing);

        var turnoNuevo = TestObjects.Turno(id: turnoNuevoId, pacienteId: TestObjects.PacienteId, profesionalId: TestObjects.ProfesionalId);
        _turnoRepository.ObtenerPorId(turnoNuevoId).Returns(turnoNuevo);
        _informeRepository.ExisteParaTurnoAsync(turnoNuevoId, informeId).Returns(true);

        var dto = new InformeUpdateDto
        {
            PacienteId = TestObjects.PacienteId,
            ProfesionalId = TestObjects.ProfesionalId,
            TurnoId = turnoNuevoId,
            Fecha = new DateOnly(2035, 1, 8),
            Tipo = "Evolución",
            Contenido = "Editado",
            Estado = "Completado"
        };

        // Act
        var act = () => CrearServicio().UpdateAsync(informeId, dto);

        // Assert
        await act.Should().ThrowAsync<ConflictError>()
            .WithMessage("*ya tiene un informe asignado*");
        await _informeRepository.DidNotReceive().UpdateAsync(Arg.Any<Informe>());
    }

    [Fact]
    public async Task GetByTurnoIdAsync_ExisteInforme_RetornaDto()
    {
        // Arrange
        const int turnoId = 15;
        var informe = TestObjects.Informe(id: 7, turnoId: turnoId);
        _informeRepository.GetByTurnoIdAsync(turnoId).Returns(informe);

        // Act
        var result = await CrearServicio().GetByTurnoIdAsync(turnoId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(7);
        result.TurnoId.Should().Be(turnoId);
    }

    [Fact]
    public async Task GetByTurnoIdAsync_NoExisteInforme_RetornaNull()
    {
        // Arrange
        const int turnoId = 15;
        _informeRepository.GetByTurnoIdAsync(turnoId).Returns((Informe?)null);

        // Act
        var result = await CrearServicio().GetByTurnoIdAsync(turnoId);

        // Assert
        result.Should().BeNull();
    }
}
