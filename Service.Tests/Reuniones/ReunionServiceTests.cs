using AutoMapper;
using Biblioteca.Entities;
using Biblioteca.Repository;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Repository.Profesionales;
using Repository.Reuniones;
using Service.Reuniones;
using Service.Tests.TestSupport;
using Utils.DTOs.Reunion;
using Utils.Exceptions;
using Xunit;

namespace Service.Tests.Reuniones;

public class ReunionServiceTests
{
    private readonly IReunionRepository _reunionRepository = Substitute.For<IReunionRepository>();
    private readonly IProfesionalRepository _profesionalRepository = Substitute.For<IProfesionalRepository>();
    private readonly IMapper _mapper = MapperStub.Create();

    private ReunionService CrearServicio() =>
        new(_reunionRepository, _profesionalRepository, _mapper);

    private static ReunionCreateDto DtoCreacion(params int[] profesionalesIds) => new()
    {
        Titulo = "Reunión de prueba",
        FechaHora = new DateTime(2035, 1, 8, 13, 0, 0, DateTimeKind.Utc),
        Modalidad = "Virtual",
        Descripcion = "Notas",
        ProfesionalId = TestObjects.ProfesionalId,
        ProfesionalesIds = profesionalesIds.ToList()
    };

    [Fact]
    public async Task AddAsync_ConProfesionalesValidos_CreaAsignaciones()
    {
        // Arrange
        var profesional3 = TestObjects.Profesional(3);
        var profesional5 = TestObjects.Profesional(5);
        _profesionalRepository
            .GetByIdsAsync(Arg.Any<IEnumerable<int>>())
            .Returns(new List<Profesional> { profesional3, profesional5 });
        _reunionRepository
            .AddAsync(Arg.Any<Reunion>())
            .Returns(ci =>
            {
                var reunion = ci.Arg<Reunion>();
                reunion.Id = 10;
                return reunion;
            });

        // Act
        var resultado = await CrearServicio().AddAsync(DtoCreacion(3, 5));

        // Assert
        resultado.Profesionales.Should().HaveCount(2);
        resultado.Profesionales.Select(p => p.Id).Should().BeEquivalentTo(new[] { 3, 5 });
        await _reunionRepository.Received(1).AddAsync(
            Arg.Is<Reunion>(r => r.Profesionales.Count == 2));
        await _reunionRepository.Received(1).GuardarCambios();
    }

    [Fact]
    public async Task AddAsync_SinProfesionales_LanzaValidationError()
    {
        // Act
        var act = () => CrearServicio().AddAsync(DtoCreacion());

        // Assert
        await act.Should().ThrowAsync<ValidationError>()
            .WithMessage("*al menos un profesional*");
        await _reunionRepository.DidNotReceive().AddAsync(Arg.Any<Reunion>());
    }

    [Fact]
    public async Task AddAsync_ConProfesionalInexistente_LanzaValidationError()
    {
        // Arrange
        var profesional3 = TestObjects.Profesional(3);
        _profesionalRepository
            .GetByIdsAsync(Arg.Any<IEnumerable<int>>())
            .Returns(new List<Profesional> { profesional3 });

        // Act
        var act = () => CrearServicio().AddAsync(DtoCreacion(3, 99));

        // Assert
        await act.Should().ThrowAsync<ValidationError>()
            .WithMessage("*99*");
        await _reunionRepository.DidNotReceive().AddAsync(Arg.Any<Reunion>());
    }

    [Fact]
    public async Task UpdateAsync_ReemplazaAsignaciones()
    {
        // Arrange
        var profesional3 = TestObjects.Profesional(3);
        var profesional5 = TestObjects.Profesional(5);
        var profesional7 = TestObjects.Profesional(7);

        var existing = new Reunion
        {
            Id = 10,
            Titulo = "Vieja",
            FechaHora = new DateTime(2035, 1, 8, 13, 0, 0, DateTimeKind.Utc),
            ProfesionalId = TestObjects.ProfesionalId,
            Profesionales = new List<ReunionProfesional>
            {
                new() { ReunionId = 10, ProfesionalId = 3, Profesional = profesional3 },
                new() { ReunionId = 10, ProfesionalId = 5, Profesional = profesional5 }
            }
        };

        _reunionRepository.GetByIdAsync(10).Returns(existing);
        _profesionalRepository
            .GetByIdsAsync(Arg.Any<IEnumerable<int>>())
            .Returns(new List<Profesional> { profesional3, profesional7 });
        _reunionRepository
            .UpdateAsync(Arg.Any<Reunion>())
            .Returns(ci => ci.Arg<Reunion>());

        var dto = new ReunionUpdateDto
        {
            Titulo = "Nueva",
            FechaHora = new DateTime(2035, 2, 10, 15, 0, 0, DateTimeKind.Utc),
            Modalidad = "Presencial",
            Descripcion = "Actualizada",
            ProfesionalId = TestObjects.ProfesionalId,
            ProfesionalesIds = new List<int> { 3, 7 }
        };

        // Act
        var resultado = await CrearServicio().UpdateAsync(10, dto);

        // Assert
        resultado.Should().NotBeNull();
        existing.Profesionales.Select(rp => rp.ProfesionalId)
            .Should().BeEquivalentTo(new[] { 3, 7 });
        resultado!.Profesionales.Select(p => p.Id)
            .Should().BeEquivalentTo(new[] { 3, 7 });
        await _reunionRepository.Received(1).UpdateAsync(existing);
        await _reunionRepository.Received(1).GuardarCambios();
    }

    [Fact]
    public async Task GetByProfesionalIdAsync_IncluyeCreadasYAsignadas()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new ApplicationDbContext(options);

        var creador3 = TestObjects.Profesional(3);
        var creador4 = TestObjects.Profesional(4);
        var asignado5 = TestObjects.Profesional(5);

        context.Profesionales.AddRange(creador3, creador4, asignado5);
        context.Reuniones.AddRange(
            new Reunion
            {
                Id = 1,
                Titulo = "Creada por 3",
                FechaHora = new DateTime(2035, 1, 8, 10, 0, 0, DateTimeKind.Utc),
                ProfesionalId = 3,
                Profesionales = new List<ReunionProfesional>
                {
                    new() { ReunionId = 1, ProfesionalId = 5, Profesional = asignado5 }
                }
            },
            new Reunion
            {
                Id = 2,
                Titulo = "Asignada a 3",
                FechaHora = new DateTime(2035, 1, 9, 10, 0, 0, DateTimeKind.Utc),
                ProfesionalId = 4,
                Profesionales = new List<ReunionProfesional>
                {
                    new() { ReunionId = 2, ProfesionalId = 3, Profesional = creador3 }
                }
            },
            new Reunion
            {
                Id = 3,
                Titulo = "Ajena a 3",
                FechaHora = new DateTime(2035, 1, 10, 10, 0, 0, DateTimeKind.Utc),
                ProfesionalId = 4,
                Profesionales = new List<ReunionProfesional>
                {
                    new() { ReunionId = 3, ProfesionalId = 5, Profesional = asignado5 }
                }
            });

        await context.SaveChangesAsync();
        context.ReunionProfesionales.Count().Should().Be(3);
        var repository = new ReunionRepository(context);

        // Act
        var resultado = (await repository.GetByProfesionalIdAsync(3)).ToList();

        // Assert
        resultado.Select(r => r.Id).Should().BeEquivalentTo(new[] { 1, 2 });
        resultado.Should().OnlyContain(r => r.Profesionales.Count > 0);
    }
}
