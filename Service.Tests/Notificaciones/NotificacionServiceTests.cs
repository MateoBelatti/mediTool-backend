using AutoMapper;
using NSubstitute;
using Repository.Notificaciones;
using Repository.Profesionales;
using Service.Notificaciones;
using Service.Tests.TestSupport;
using Utils.DTOs.Notificacion;
using Utils.Enums;
using Utils.Exceptions;

namespace Service.Tests.Notificaciones;

public class NotificacionServiceTests
{
    private readonly INotificacionRepository _notificacionRepository = Substitute.For<INotificacionRepository>();
    private readonly IProfesionalRepository _profesionalRepository = Substitute.For<IProfesionalRepository>();
    private readonly IMapper _mapper = MapperStub.Create();

    private NotificacionService CrearServicio() =>
        new(_notificacionRepository, _profesionalRepository, _mapper);

    private static CrearNotificacionDto DtoValido() => new()
    {
        ProfesionalId = TestObjects.ProfesionalId,
        Tipo = TipoNotificacion.TurnosSinAsistencia,
        Titulo = "Turnos sin asistencia",
        Mensaje = "Tenés 3 turnos sin registrar asistencia."
    };

    // ----- ObtenerPorProfesionalAsync -----

    [Fact]
    public async Task ObtenerPorProfesionalAsync_ConNotificaciones_DevuelvePageResultMapeado()
    {
        // Arrange
        var lista = new List<Biblioteca.Entities.Notificacion>
        {
            TestObjects.Notificacion(id: 1),
            TestObjects.Notificacion(id: 2, leida: true)
        };
        _notificacionRepository.ObtenerPorProfesional(TestObjects.ProfesionalId, 1, 10, null)
            .Returns((lista, 2));

        // Act
        var resultado = await CrearServicio().ObtenerPorProfesionalAsync(TestObjects.ProfesionalId, 1, 10);

        // Assert
        resultado.Items.Should().HaveCount(2);
        resultado.Page.Should().Be(1);
        resultado.PageSize.Should().Be(10);
        resultado.TotalItems.Should().Be(2);
        resultado.TotalPages.Should().Be(1);
        resultado.Items.Should().Contain(n => n.Id == 1 && !n.Leida);
    }

    [Fact]
    public async Task ObtenerPorProfesionalAsync_MasDeUnaPagina_CalculaTotalPages()
    {
        // Arrange
        var lista = Enumerable.Range(1, 10)
            .Select(i => TestObjects.Notificacion(id: i))
            .ToList();
        _notificacionRepository.ObtenerPorProfesional(TestObjects.ProfesionalId, 1, 10, soloNoLeidas: true)
            .Returns((lista, 25));

        // Act
        var resultado = await CrearServicio().ObtenerPorProfesionalAsync(TestObjects.ProfesionalId, 1, 10, soloNoLeidas: true);

        // Assert
        resultado.TotalItems.Should().Be(25);
        resultado.TotalPages.Should().Be(3);
    }

    // ----- ContarNoLeidasAsync -----

    [Fact]
    public async Task ContarNoLeidasAsync_DelegaAlRepositorioConElProfesional()
    {
        // Arrange
        _notificacionRepository.ContarNoLeidas(TestObjects.ProfesionalId).Returns(3);

        // Act
        var cantidad = await CrearServicio().ContarNoLeidasAsync(TestObjects.ProfesionalId);

        // Assert
        cantidad.Should().Be(3);
        await _notificacionRepository.Received(1).ContarNoLeidas(TestObjects.ProfesionalId);
    }

    // ----- CrearAsync -----

    [Fact]
    public async Task CrearAsync_DevuelveDtoYPersiste()
    {
        // Arrange
        var dto = DtoValido();
        _profesionalRepository.GetByIdAsync(dto.ProfesionalId).Returns(TestObjects.Profesional());
        _notificacionRepository.Crear(Arg.Any<Biblioteca.Entities.Notificacion>())
            .Returns(ci => ci.Arg<Biblioteca.Entities.Notificacion>());

        // Act
        var resultado = await CrearServicio().CrearAsync(dto);

        // Assert
        resultado.Titulo.Should().Be(dto.Titulo);
        resultado.Mensaje.Should().Be(dto.Mensaje);
        resultado.Tipo.Should().Be(dto.Tipo);
        resultado.ProfesionalId.Should().Be(dto.ProfesionalId);
        await _notificacionRepository.Received(1).Crear(Arg.Any<Biblioteca.Entities.Notificacion>());
        await _notificacionRepository.Received(1).GuardarCambios();
    }

    [Fact]
    public async Task CrearAsync_ProfesionalInexistente_LanzaNotFoundError()
    {
        // Arrange
        var dto = DtoValido();
        _profesionalRepository.GetByIdAsync(dto.ProfesionalId).Returns((Biblioteca.Entities.Profesional?)null);

        // Act
        var accion = () => CrearServicio().CrearAsync(dto);

        // Assert
        await accion.Should().ThrowAsync<NotFoundError>();
        await _notificacionRepository.DidNotReceive().Crear(Arg.Any<Biblioteca.Entities.Notificacion>());
    }

    // ----- MarcarComoLeidaAsync -----

    [Fact]
    public async Task MarcarComoLeidaAsync_NotificacionExistente_Persiste()
    {
        // Arrange
        _notificacionRepository.MarcarComoLeida(5, TestObjects.ProfesionalId).Returns(true);

        // Act
        var resultado = await CrearServicio().MarcarComoLeidaAsync(5, TestObjects.ProfesionalId);

        // Assert
        resultado.Should().BeTrue();
        await _notificacionRepository.Received(1).MarcarComoLeida(5, TestObjects.ProfesionalId);
        await _notificacionRepository.Received(1).GuardarCambios();
    }

    [Fact]
    public async Task MarcarComoLeidaAsync_NotificacionInexistenteOAjena_LanzaNotFoundError()
    {
        // Arrange: repo devuelve false cuando no existe o pertenece a otro profesional
        _notificacionRepository.MarcarComoLeida(99, TestObjects.ProfesionalId).Returns(false);

        // Act
        var accion = () => CrearServicio().MarcarComoLeidaAsync(99, TestObjects.ProfesionalId);

        // Assert
        await accion.Should().ThrowAsync<NotFoundError>();
        await _notificacionRepository.DidNotReceive().GuardarCambios();
    }

    // ----- MarcarTodasLeidasAsync -----

    [Fact]
    public async Task MarcarTodasLeidasAsync_DevuelveLaCantidadActualizada()
    {
        // Arrange
        _notificacionRepository.MarcarTodasLeidas(TestObjects.ProfesionalId).Returns(4);

        // Act
        var cantidad = await CrearServicio().MarcarTodasLeidasAsync(TestObjects.ProfesionalId);

        // Assert
        cantidad.Should().Be(4);
        await _notificacionRepository.Received(1).MarcarTodasLeidas(TestObjects.ProfesionalId);
        await _notificacionRepository.DidNotReceive().GuardarCambios();
    }
}
