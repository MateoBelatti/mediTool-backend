using AutoMapper;
using Biblioteca.Entities;
using NSubstitute;
using Repository.Almacenamiento;
using Repository.Documentos;
using Repository.Pacientes;
using Service.Documentos;
using Service.Tests.TestSupport;
using Utils.DTOs.Documento;
using Utils.Exceptions;

namespace Service.Tests.Documentos;

public class DocumentoPacienteServiceTests
{
    private const string ClaveStorage = "clave-test";
    private const int DocumentoId = 50;

    private readonly IDocumentoPacienteRepository _documentoRepository = Substitute.For<IDocumentoPacienteRepository>();
    private readonly IPacienteRepository _pacienteRepository = Substitute.For<IPacienteRepository>();
    private readonly IArchivoStorage _archivoStorage = Substitute.For<IArchivoStorage>();
    private readonly IMapper _mapper = MapperStub.Create();

    private DocumentoPacienteService CrearServicio() =>
        new(_documentoRepository, _pacienteRepository, _archivoStorage, _mapper);

    private void SetupPacienteExistente() =>
        _pacienteRepository.GetByIdAsync(TestObjects.PacienteId).Returns(TestObjects.Paciente());

    private void SetupGuardadoExitoso()
    {
        _archivoStorage.GuardarAsync(Arg.Any<Stream>())
            .Returns(new ArchivoGuardado(ClaveStorage, 12));

        _documentoRepository.AddAsync(Arg.Any<DocumentoPaciente>())
            .Returns(ci =>
            {
                var entity = ci.Arg<DocumentoPaciente>();
                entity.Id = DocumentoId;
                return entity;
            });

        _documentoRepository.GetByIdAsync(DocumentoId)
            .Returns(ci => DocumentoEntity());
    }

    private static DocumentoPaciente DocumentoEntity(string clave = ClaveStorage) => new()
    {
        Id = DocumentoId,
        PacienteId = TestObjects.PacienteId,
        ProfesionalId = TestObjects.ProfesionalId,
        ArchivoClave = clave,
        NombreOriginal = "informe.pdf",
        Extension = ".pdf",
        ContentType = "application/pdf",
        TamanoBytes = 12,
        FechaSubida = new DateTime(2035, 1, 8, 10, 0, 0, DateTimeKind.Utc),
        Profesional = TestObjects.Profesional()
    };

    private static DocumentoSubidaDto DtoPdf(string nombreArchivo = "informe.pdf")
    {
        var contenido = PdfStream();
        return new DocumentoSubidaDto
        {
            Contenido = contenido,
            NombreArchivo = nombreArchivo,
            ContentType = "application/pdf",
            TamanoBytes = contenido.Length
        };
    }

    private static MemoryStream PdfStream() =>
        new("%PDF-1.4\ncontenido de prueba"u8.ToArray());

    private static MemoryStream DocxStream() =>
        new([0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x00, 0x00]);

    private static MemoryStream DocStream() =>
        new([0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1]);

    [Fact]
    public async Task AddAsync_PdfValido_GuardaArchivoYMetadata()
    {
        // Arrange
        SetupPacienteExistente();
        SetupGuardadoExitoso();
        var dto = DtoPdf();

        // Act
        var result = await CrearServicio().AddAsync(TestObjects.PacienteId, TestObjects.ProfesionalId, dto);

        // Assert
        result.Should().NotBeNull();
        result.NombreOriginal.Should().Be("informe.pdf");
        result.Extension.Should().Be(".pdf");
        result.ContentType.Should().Be("application/pdf");
        result.ProfesionalNombre.Should().Be("Ana Gomez");

        await _archivoStorage.Received(1).GuardarAsync(Arg.Any<Stream>());
        await _documentoRepository.Received(1).AddAsync(Arg.Is<DocumentoPaciente>(d =>
            d.PacienteId == TestObjects.PacienteId &&
            d.ProfesionalId == TestObjects.ProfesionalId &&
            d.ArchivoClave == ClaveStorage &&
            d.NombreOriginal == "informe.pdf" &&
            d.Extension == ".pdf"));
        await _documentoRepository.Received(1).GuardarCambios();
    }

    [Fact]
    public async Task AddAsync_WordDocxValido_GuardaArchivo()
    {
        // Arrange
        SetupPacienteExistente();
        SetupGuardadoExitoso();
        var contenido = DocxStream();
        var dto = new DocumentoSubidaDto
        {
            Contenido = contenido,
            NombreArchivo = "estudio.docx",
            ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            TamanoBytes = contenido.Length
        };

        // Act
        var result = await CrearServicio().AddAsync(TestObjects.PacienteId, TestObjects.ProfesionalId, dto);

        // Assert
        result.Should().NotBeNull();
        await _documentoRepository.Received(1).AddAsync(Arg.Is<DocumentoPaciente>(d =>
            d.Extension == ".docx" &&
            d.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document"));
    }

    [Fact]
    public async Task AddAsync_ArchivoMayorA10Mb_LanzaValidationError()
    {
        // Arrange
        var dto = DtoPdf();
        dto.TamanoBytes = DocumentoPacienteService.MaxArchivoBytes + 1;

        // Act
        var act = () => CrearServicio().AddAsync(TestObjects.PacienteId, TestObjects.ProfesionalId, dto);

        // Assert
        await act.Should().ThrowAsync<ValidationError>()
            .WithMessage("*tamaño máximo*");
        await _archivoStorage.DidNotReceive().GuardarAsync(Arg.Any<Stream>());
    }

    [Fact]
    public async Task AddAsync_ExtensionNoPermitida_LanzaValidationError()
    {
        // Arrange
        var dto = DtoPdf("virus.exe");

        // Act
        var act = () => CrearServicio().AddAsync(TestObjects.PacienteId, TestObjects.ProfesionalId, dto);

        // Assert
        await act.Should().ThrowAsync<ValidationError>()
            .WithMessage("*PDF o Word*");
        await _archivoStorage.DidNotReceive().GuardarAsync(Arg.Any<Stream>());
    }

    [Fact]
    public async Task AddAsync_TipoContenidoNoCoincide_LanzaValidationError()
    {
        // Arrange
        var dto = DtoPdf();
        dto.ContentType = "image/png";

        // Act
        var act = () => CrearServicio().AddAsync(TestObjects.PacienteId, TestObjects.ProfesionalId, dto);

        // Assert
        await act.Should().ThrowAsync<ValidationError>()
            .WithMessage("*tipo de contenido*");
        await _archivoStorage.DidNotReceive().GuardarAsync(Arg.Any<Stream>());
    }

    [Fact]
    public async Task AddAsync_ContenidoNoCoincideConExtension_LanzaValidationError()
    {
        // Arrange
        SetupPacienteExistente();
        var dto = DtoPdf();
        dto.Contenido = new MemoryStream("no es un pdf"u8.ToArray());
        dto.TamanoBytes = dto.Contenido.Length;

        // Act
        var act = () => CrearServicio().AddAsync(TestObjects.PacienteId, TestObjects.ProfesionalId, dto);

        // Assert
        await act.Should().ThrowAsync<ValidationError>()
            .WithMessage("*no corresponde a un PDF o Word válido*");
        await _archivoStorage.DidNotReceive().GuardarAsync(Arg.Any<Stream>());
    }

    [Fact]
    public async Task AddAsync_PacienteInexistente_LanzaNotFoundError()
    {
        // Arrange
        _pacienteRepository.GetByIdAsync(TestObjects.PacienteId).Returns((Paciente?)null);
        var dto = DtoPdf();

        // Act
        var act = () => CrearServicio().AddAsync(TestObjects.PacienteId, TestObjects.ProfesionalId, dto);

        // Assert
        await act.Should().ThrowAsync<NotFoundError>();
        await _archivoStorage.DidNotReceive().GuardarAsync(Arg.Any<Stream>());
    }

    [Fact]
    public async Task AddAsync_FallaGuardadoMetadata_EliminaArchivoAlmacenado()
    {
        // Arrange
        SetupPacienteExistente();
        _archivoStorage.GuardarAsync(Arg.Any<Stream>())
            .Returns(new ArchivoGuardado(ClaveStorage, 12));
        _documentoRepository.AddAsync(Arg.Any<DocumentoPaciente>())
            .Returns(ci => Task.FromException<DocumentoPaciente>(new InvalidOperationException("falló el guardado")));
        var dto = DtoPdf();

        // Act
        var act = () => CrearServicio().AddAsync(TestObjects.PacienteId, TestObjects.ProfesionalId, dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        await _archivoStorage.Received(1).EliminarAsync(ClaveStorage, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByPacienteAsync_PacienteConDocumentos_DevuelveListado()
    {
        // Arrange
        _documentoRepository.GetByPacienteAsync(TestObjects.PacienteId)
            .Returns([DocumentoEntity()]);

        // Act
        var result = await CrearServicio().GetByPacienteAsync(TestObjects.PacienteId);

        // Assert
        result.Should().ContainSingle();
        result.First().Id.Should().Be(DocumentoId);
    }

    [Fact]
    public async Task GetArchivoAsync_DocumentoExistente_DevuelveContenidoYMetadata()
    {
        // Arrange
        var contenido = new MemoryStream([1, 2, 3]);
        _documentoRepository.GetByIdAsync(DocumentoId).Returns(DocumentoEntity());
        _archivoStorage.AbrirAsync(ClaveStorage).Returns(contenido);

        // Act
        var result = await CrearServicio().GetArchivoAsync(TestObjects.PacienteId, DocumentoId);

        // Assert
        result.Should().NotBeNull();
        result!.Documento.Id.Should().Be(DocumentoId);
        result.Contenido.Should().BeSameAs(contenido);
    }

    [Fact]
    public async Task GetArchivoAsync_DocumentoDeOtroPaciente_DevuelveNull()
    {
        // Arrange
        _documentoRepository.GetByIdAsync(DocumentoId).Returns(DocumentoEntity());

        // Act
        var result = await CrearServicio().GetArchivoAsync(999, DocumentoId);

        // Assert
        result.Should().BeNull();
        await _archivoStorage.DidNotReceive().AbrirAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_DocumentoExistente_EliminaMetadataYArchivo()
    {
        // Arrange
        var documento = DocumentoEntity();
        _documentoRepository.GetByIdAsync(DocumentoId).Returns(documento);
        _documentoRepository.DeleteAsync(documento).Returns(true);

        // Act
        var result = await CrearServicio().DeleteAsync(TestObjects.PacienteId, DocumentoId);

        // Assert
        result.Should().BeTrue();
        await _documentoRepository.Received(1).DeleteAsync(documento);
        await _documentoRepository.Received(1).GuardarCambios();
        await _archivoStorage.Received(1).EliminarAsync(ClaveStorage, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_DocumentoInexistente_DevuelveFalse()
    {
        // Arrange
        _documentoRepository.GetByIdAsync(DocumentoId).Returns((DocumentoPaciente?)null);

        // Act
        var result = await CrearServicio().DeleteAsync(TestObjects.PacienteId, DocumentoId);

        // Assert
        result.Should().BeFalse();
        await _archivoStorage.DidNotReceive().EliminarAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
