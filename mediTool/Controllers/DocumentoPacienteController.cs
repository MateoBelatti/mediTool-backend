using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Documentos;
using Service.Pacientes;
using Utils.DTOs.Documento;
using Utils.Exceptions;
using Utils.Helpers;

namespace mediTool.Controllers
{
    [Authorize]
    [Route("api/Paciente/{pacienteId:int}/documentos")]
    [ApiController]
    public class DocumentoPacienteController : ControllerBase
    {
        private readonly IDocumentoPacienteService _documentoService;
        private readonly IPacienteService _pacienteService;

        public DocumentoPacienteController(
            IDocumentoPacienteService documentoService,
            IPacienteService pacienteService)
        {
            _documentoService = documentoService;
            _pacienteService = pacienteService;
        }

        [Authorize(Policy = "AdminOrProfesional")]
        [HttpGet]
        public async Task<IActionResult> GetAll(int pacienteId, CancellationToken cancellationToken)
        {
            await EnsureAccesoAsync(pacienteId);

            var documentos = await _documentoService.GetByPacienteAsync(pacienteId, cancellationToken);
            return Ok(documentos);
        }

        [Authorize(Policy = "AdminOrProfesional")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(11 * 1024 * 1024)]
        public async Task<IActionResult> Upload(int pacienteId, [FromForm] IFormFile? archivo, CancellationToken cancellationToken)
        {
            await EnsureAccesoAsync(pacienteId);

            if (archivo == null || archivo.Length == 0)
                throw new ValidationError("Debe adjuntar un archivo PDF o Word.");

            await using var contenido = archivo.OpenReadStream();
            var dto = new DocumentoSubidaDto
            {
                Contenido = contenido,
                NombreArchivo = archivo.FileName,
                ContentType = archivo.ContentType,
                TamanoBytes = archivo.Length
            };

            var result = await _documentoService.AddAsync(pacienteId, User.GetUserId(), dto, cancellationToken);
            return CreatedAtAction(nameof(GetArchivo), new { pacienteId, documentoId = result.Id }, result);
        }

        [Authorize(Policy = "AdminOrProfesional")]
        [HttpGet("{documentoId:int}/archivo")]
        public async Task<IActionResult> GetArchivo(int pacienteId, int documentoId, CancellationToken cancellationToken)
        {
            await EnsureAccesoAsync(pacienteId);

            var result = await _documentoService.GetArchivoAsync(pacienteId, documentoId, cancellationToken);
            if (result == null)
                return NotFound();

            return File(
                result.Contenido,
                result.Documento.ContentType,
                result.Documento.NombreOriginal,
                enableRangeProcessing: true);
        }

        [Authorize(Policy = "AdminOrProfesional")]
        [HttpDelete("{documentoId:int}")]
        public async Task<IActionResult> Delete(int pacienteId, int documentoId, CancellationToken cancellationToken)
        {
            await EnsureAccesoAsync(pacienteId);

            var success = await _documentoService.DeleteAsync(pacienteId, documentoId, cancellationToken);
            if (!success)
                return NotFound();

            return NoContent();
        }

        private async Task EnsureAccesoAsync(int pacienteId)
        {
            if (User.IsAdmin()) return;

            if (!await _pacienteService.IsVinculadoAsync(pacienteId, User.GetUserId()))
                throw new ForbiddenError("No tiene permisos para acceder a este recurso.");
        }
    }
}
