using AutoMapper;
using Biblioteca.Entities;
using Repository.Almacenamiento;
using Repository.Documentos;
using Repository.Pacientes;
using Utils.DTOs.Documento;
using Utils.Exceptions;

namespace Service.Documentos
{
    public class DocumentoPacienteService : IDocumentoPacienteService
    {
        public const long MaxArchivoBytes = 25 * 1024 * 1024;

        private static readonly HashSet<string> ExtensionesPermitidas = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf",
            ".doc",
            ".docx"
        };

        private static readonly Dictionary<string, string> MimePorExtension = new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "application/pdf",
            [".doc"] = "application/msword",
            [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };

        private readonly IDocumentoPacienteRepository _documentoRepository;
        private readonly IPacienteRepository _pacienteRepository;
        private readonly IArchivoStorage _archivoStorage;
        private readonly IMapper _mapper;

        public DocumentoPacienteService(
            IDocumentoPacienteRepository documentoRepository,
            IPacienteRepository pacienteRepository,
            IArchivoStorage archivoStorage,
            IMapper mapper)
        {
            _documentoRepository = documentoRepository;
            _pacienteRepository = pacienteRepository;
            _archivoStorage = archivoStorage;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DocumentoPacienteResponseDto>> GetByPacienteAsync(int pacienteId, CancellationToken cancellationToken = default)
        {
            if (pacienteId <= 0)
                throw new ValidationError("El id del paciente debe ser mayor que 0.");

            var documentos = await _documentoRepository.GetByPacienteAsync(pacienteId, cancellationToken);
            return _mapper.Map<IEnumerable<DocumentoPacienteResponseDto>>(documentos);
        }

        public async Task<ArchivoDocumento?> GetArchivoAsync(int pacienteId, int documentoId, CancellationToken cancellationToken = default)
        {
            if (pacienteId <= 0)
                throw new ValidationError("El id del paciente debe ser mayor que 0.");
            if (documentoId <= 0)
                throw new ValidationError("El id del documento debe ser mayor que 0.");

            var documento = await _documentoRepository.GetByIdAsync(documentoId, cancellationToken);
            if (documento == null || documento.PacienteId != pacienteId) return null;

            var contenido = await _archivoStorage.AbrirAsync(documento.ArchivoClave, cancellationToken);
            if (contenido == null)
                throw new NotFoundError("El archivo del documento no se encontró en el almacenamiento.");

            return new ArchivoDocumento(_mapper.Map<DocumentoPacienteResponseDto>(documento), contenido);
        }

        public async Task<DocumentoPacienteResponseDto> AddAsync(int pacienteId, int profesionalId, DocumentoSubidaDto dto, CancellationToken cancellationToken = default)
        {
            if (pacienteId <= 0)
                throw new ValidationError("El id del paciente debe ser mayor que 0.");
            if (profesionalId <= 0)
                throw new ValidationError("El id del profesional debe ser mayor que 0.");
            if (dto == null || dto.Contenido == null)
                throw new ValidationError("No se recibió ningún archivo.");
            if (dto.TamanoBytes <= 0)
                throw new ValidationError("El archivo está vacío.");
            if (dto.TamanoBytes > MaxArchivoBytes)
                throw new ValidationError("El archivo supera el tamaño máximo permitido de 25 MB.");

            var nombreOriginal = SanitizarNombre(dto.NombreArchivo);
            var extension = Path.GetExtension(nombreOriginal).ToLowerInvariant();

            if (!ExtensionesPermitidas.Contains(extension))
                throw new ValidationError("Solo se permiten archivos PDF o Word (.doc, .docx).");

            var mimeEsperado = MimePorExtension[extension];
            if (!string.IsNullOrWhiteSpace(dto.ContentType) &&
                !string.Equals(dto.ContentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(dto.ContentType, mimeEsperado, StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationError("El tipo de contenido no coincide con la extensión del archivo.");
            }

            var paciente = await _pacienteRepository.GetByIdAsync(pacienteId);
            if (paciente == null)
                throw new NotFoundError($"Paciente {pacienteId} no encontrado.");

            using var buffer = new MemoryStream();
            await dto.Contenido.CopyToAsync(buffer, cancellationToken);
            var bytes = buffer.ToArray();

            if (bytes.Length == 0)
                throw new ValidationError("El archivo está vacío.");
            if (bytes.LongLength > MaxArchivoBytes)
                throw new ValidationError("El archivo supera el tamaño máximo permitido de 25 MB.");
            if (!FirmaValida(extension, bytes))
                throw new ValidationError("El contenido del archivo no corresponde a un PDF o Word válido.");

            var guardado = await _archivoStorage.GuardarAsync(new MemoryStream(bytes), cancellationToken);

            try
            {
                var entity = new DocumentoPaciente
                {
                    PacienteId = pacienteId,
                    ProfesionalId = profesionalId,
                    ArchivoClave = guardado.Clave,
                    NombreOriginal = nombreOriginal,
                    Extension = extension,
                    ContentType = string.IsNullOrWhiteSpace(dto.ContentType) || string.Equals(dto.ContentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase)
                        ? mimeEsperado
                        : dto.ContentType,
                    TamanoBytes = guardado.TamanoBytes,
                    FechaSubida = DateTime.UtcNow
                };

                var creado = await _documentoRepository.AddAsync(entity, cancellationToken);
                await _documentoRepository.GuardarCambios(cancellationToken);

                var result = await _documentoRepository.GetByIdAsync(creado.Id, cancellationToken);
                return _mapper.Map<DocumentoPacienteResponseDto>(result ?? creado);
            }
            catch
            {
                await _archivoStorage.EliminarAsync(guardado.Clave, CancellationToken.None);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int pacienteId, int documentoId, CancellationToken cancellationToken = default)
        {
            if (pacienteId <= 0)
                throw new ValidationError("El id del paciente debe ser mayor que 0.");
            if (documentoId <= 0)
                throw new ValidationError("El id del documento debe ser mayor que 0.");

            var documento = await _documentoRepository.GetByIdAsync(documentoId, cancellationToken);
            if (documento == null || documento.PacienteId != pacienteId) return false;

            var eliminado = await _documentoRepository.DeleteAsync(documento, cancellationToken);
            if (!eliminado) return false;

            await _documentoRepository.GuardarCambios(cancellationToken);
            await _archivoStorage.EliminarAsync(documento.ArchivoClave, cancellationToken);
            return true;
        }

        private static string SanitizarNombre(string? nombreArchivo)
        {
            var nombre = (nombreArchivo ?? string.Empty).Trim().Replace('\\', '/');
            nombre = Path.GetFileName(nombre);

            if (string.IsNullOrWhiteSpace(nombre))
                throw new ValidationError("El nombre del archivo es obligatorio.");

            if (nombre.Length > 255)
            {
                var extension = Path.GetExtension(nombre);
                var baseNombre = Path.GetFileNameWithoutExtension(nombre);
                var maxBase = Math.Max(255 - extension.Length, 0);
                nombre = baseNombre[..Math.Min(baseNombre.Length, maxBase)] + extension;
            }

            return nombre;
        }

        private static bool FirmaValida(string extension, byte[] contenido)
        {
            return extension switch
            {
                ".pdf" => contenido.Length >= 5 &&
                          contenido[0] == 0x25 &&
                          contenido[1] == 0x50 &&
                          contenido[2] == 0x44 &&
                          contenido[3] == 0x46 &&
                          contenido[4] == 0x2D,
                ".docx" => contenido.Length >= 4 &&
                           contenido[0] == 0x50 &&
                           contenido[1] == 0x4B &&
                           contenido[2] == 0x03 &&
                           contenido[3] == 0x04,
                ".doc" => contenido.Length >= 8 &&
                          contenido[0] == 0xD0 &&
                          contenido[1] == 0xCF &&
                          contenido[2] == 0x11 &&
                          contenido[3] == 0xE0 &&
                          contenido[4] == 0xA1 &&
                          contenido[5] == 0xB1 &&
                          contenido[6] == 0x1A &&
                          contenido[7] == 0xE1,
                _ => false
            };
        }
    }
}
