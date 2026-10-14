using AutoMapper;
using Biblioteca.Entities;
using Biblioteca.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NSubstitute;
using Repository.Pacientes;
using Repository.TurnosFijos;
using Service.Tests.TestSupport;
using Service.Turnos;
using Service.TurnosFijos;
using Utils.DTOs.TurnoFijo;
using Utils.Exceptions;

namespace Service.Tests.TurnosFijos;

public class TurnoFijoServiceTests
{
    private readonly ITurnoFijoRepository _turnoFijoRepository = Substitute.For<ITurnoFijoRepository>();
    private readonly IGenerarInstanciasTurnoService _generarInstancias = Substitute.For<IGenerarInstanciasTurnoService>();
    private readonly IPacienteRepository _pacienteRepository = Substitute.For<IPacienteRepository>();
    private readonly IMapper _mapper = MapperStub.Create();
    private readonly ApplicationDbContext _dbContext;

    public TurnoFijoServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _dbContext = new ApplicationDbContext(options);
    }

    private TurnoFijoService CrearServicio() =>
        new(_turnoFijoRepository, _generarInstancias, _pacienteRepository, _dbContext, _mapper);

    private static CrearTurnoFijoDto DtoValido() => new()
    {
        PacienteId = TestObjects.PacienteId,
        ProfesionalId = TestObjects.ProfesionalId,
        DiaSemana = (int)DayOfWeek.Monday,
        Hora = new TimeSpan(10, 0, 0),
        DuracionMin = 30,
        FechaInicio = new DateOnly(2035, 1, 1),
        Activo = true
    };

    private void Vinculado(bool vinculado = true) =>
        _pacienteRepository.IsVinculadoAsync(TestObjects.PacienteId, TestObjects.ProfesionalId).Returns(vinculado);

    // ----- Crear: camino feliz -----

    [Fact]
    public async Task Crear_DatosValidos_RegistraLaReglaYGeneraInstancias()
    {
        // Arrange
        Vinculado();
        _generarInstancias.GenerarInstancias(Arg.Any<TurnoFijo>(), Arg.Any<DateTime>()).Returns(5);

        // Act
        var resultado = await CrearServicio().Crear(DtoValido());

        // Assert
        resultado.Should().NotBeNull();
        await _turnoFijoRepository.Received(1).Agregar(Arg.Is<TurnoFijo>(
            r => r.PacienteId == TestObjects.PacienteId && r.ProfesionalId == TestObjects.ProfesionalId));
        await _turnoFijoRepository.Received(1).GuardarCambios();
        await _generarInstancias.Received(1).GenerarInstancias(Arg.Any<TurnoFijo>(), Arg.Any<DateTime>());
    }

    [Fact]
    public async Task Crear_SinHorizonteExplicito_GeneraConElHorizontePorDefectoDeTresMeses()
    {
        // Arrange
        Vinculado();
        _generarInstancias.GenerarInstancias(Arg.Any<TurnoFijo>(), Arg.Any<DateTime>()).Returns(5);
        DateTime? horizonteUsado = null;
        await _generarInstancias.GenerarInstancias(
            Arg.Any<TurnoFijo>(),
            Arg.Do<DateTime>(h => horizonteUsado = h));

        var esperado = DateTime.UtcNow.AddMonths(3);

        // Act
        await CrearServicio().Crear(DtoValido());

        // Assert
        horizonteUsado.Should().NotBeNull();
        horizonteUsado!.Value.Should().BeCloseTo(esperado, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Crear_ConHorizonteExplicito_UsaElHorizonteIndicado()
    {
        // Arrange
        Vinculado();
        _generarInstancias.GenerarInstancias(Arg.Any<TurnoFijo>(), Arg.Any<DateTime>()).Returns(5);
        DateTime? horizonteUsado = null;
        await _generarInstancias.GenerarInstancias(
            Arg.Any<TurnoFijo>(),
            Arg.Do<DateTime>(h => horizonteUsado = h));

        var dto = DtoValido();
        dto.HorizonteMeses = 6;

        // Act
        await CrearServicio().Crear(dto);

        // Assert
        horizonteUsado!.Value.Should().BeCloseTo(DateTime.UtcNow.AddMonths(6), TimeSpan.FromMinutes(1));
    }

    // ----- Crear: validaciones -----

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task Crear_HorizonteInvalido_LanzaValidationError(int horizonte)
    {
        // Arrange
        var dto = DtoValido();
        dto.HorizonteMeses = horizonte;

        // Act
        var accion = () => CrearServicio().Crear(dto);

        // Assert
        await accion.Should().ThrowAsync<ValidationError>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(7)]
    public async Task Crear_DiaSemanaFueraDeRango_LanzaValidationError(int diaSemana)
    {
        // Arrange
        var dto = DtoValido();
        dto.DiaSemana = diaSemana;

        // Act
        var accion = () => CrearServicio().Crear(dto);

        // Assert
        await accion.Should().ThrowAsync<ValidationError>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task Crear_DuracionInvalida_LanzaValidationError(int duracion)
    {
        // Arrange
        var dto = DtoValido();
        dto.DuracionMin = duracion;

        // Act
        var accion = () => CrearServicio().Crear(dto);

        // Assert
        await accion.Should().ThrowAsync<ValidationError>();
    }

    [Fact]
    public async Task Crear_FechaInicioPosteriorAFechaFin_LanzaValidationError()
    {
        // Arrange
        var dto = DtoValido();
        dto.FechaInicio = new DateOnly(2035, 2, 1);
        dto.FechaFin = new DateOnly(2035, 1, 1);

        // Act
        var accion = () => CrearServicio().Crear(dto);

        // Assert
        await accion.Should().ThrowAsync<ValidationError>();
    }

    [Fact]
    public async Task Crear_PacienteNoVinculado_LanzaConflictError()
    {
        // Arrange
        Vinculado(vinculado: false);

        // Act
        var accion = () => CrearServicio().Crear(DtoValido());

        // Assert
        await accion.Should().ThrowAsync<ConflictError>();
    }

    [Fact]
    public async Task Crear_LaReglaNoGeneraInstancias_LanzaValidationError()
    {
        // Arrange
        Vinculado();
        _generarInstancias.GenerarInstancias(Arg.Any<TurnoFijo>(), Arg.Any<DateTime>()).Returns(0);

        // Act
        var accion = () => CrearServicio().Crear(DtoValido());

        // Assert
        await accion.Should().ThrowAsync<ValidationError>();
    }

    // ----- Editar -----

    [Fact]
    public async Task Editar_ReglaInexistente_LanzaNotFoundError()
    {
        // Arrange
        _turnoFijoRepository.ObtenerPorId(99).Returns((TurnoFijo?)null);

        // Act
        var accion = () => CrearServicio().Editar(99, new EditarTurnoFijoDto { DiaSemana = 1, DuracionMin = 30 });

        // Assert
        await accion.Should().ThrowAsync<NotFoundError>();
    }

    [Fact]
    public async Task Editar_DatosValidos_ActualizaLaReglaYLasInstanciasFuturas()
    {
        // Arrange
        var regla = TestObjects.TurnoFijo(id: 8, duracionMin: 30);
        _turnoFijoRepository.ObtenerPorId(8).Returns(regla);
        var dto = new EditarTurnoFijoDto { DiaSemana = 2, Hora = new TimeSpan(9, 0, 0), DuracionMin = 45, Activo = true };

        // Act
        await CrearServicio().Editar(8, dto);

        // Assert
        regla.DuracionMin.Should().Be(45);
        await _turnoFijoRepository.Received(1).Actualizar(regla);
        await _generarInstancias.Received(1).ActualizarInstanciasFuturas(regla);
    }

    // ----- Desactivar -----

    [Fact]
    public async Task Desactivar_ReglaInexistente_LanzaNotFoundError()
    {
        // Arrange
        _turnoFijoRepository.ObtenerPorId(99).Returns((TurnoFijo?)null);

        // Act
        var accion = () => CrearServicio().Desactivar(99);

        // Assert
        await accion.Should().ThrowAsync<NotFoundError>();
    }

    [Fact]
    public async Task Desactivar_ReglaExistente_LaMarcaInactivaYCancelaLasInstanciasFuturas()
    {
        // Arrange
        var regla = TestObjects.TurnoFijo(id: 8, activo: true);
        _turnoFijoRepository.ObtenerPorId(8).Returns(regla);

        // Act
        await CrearServicio().Desactivar(8);

        // Assert
        regla.Activo.Should().BeFalse();
        await _turnoFijoRepository.Received(1).Actualizar(regla);
        await _generarInstancias.Received(1).CancelarInstanciasFuturas(8);
    }

    // ----- Consultas -----

    [Fact]
    public async Task ListarPorProfesional_ProfesionalConReglas_DevuelveListaMapeada()
    {
        // Arrange
        var reglas = new List<TurnoFijo>
        {
            TestObjects.TurnoFijo(id: 1, profesionalId: TestObjects.ProfesionalId),
            TestObjects.TurnoFijo(id: 2, profesionalId: TestObjects.ProfesionalId)
        };
        _turnoFijoRepository.ObtenerPorProfesional(TestObjects.ProfesionalId).Returns(reglas);

        // Act
        var resultado = await CrearServicio().ListarPorProfesional(TestObjects.ProfesionalId);

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Select(r => r.Id).Should().Equal(1, 2);
    }

    [Fact]
    public async Task ObtenerPorId_ReglaExistente_DevuelveDto()
    {
        // Arrange
        var regla = TestObjects.TurnoFijo(id: 5);
        _turnoFijoRepository.ObtenerPorId(5).Returns(regla);

        // Act
        var resultado = await CrearServicio().ObtenerPorId(5);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(5);
    }

    [Fact]
    public async Task ObtenerPorId_ReglaInexistente_DevuelveNull()
    {
        // Arrange
        _turnoFijoRepository.ObtenerPorId(99).Returns((TurnoFijo?)null);

        // Act
        var resultado = await CrearServicio().ObtenerPorId(99);

        // Assert
        resultado.Should().BeNull();
    }
}
