using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Service.Pacientes;
using Utils.DTOs.Paciente;
using Utils.Exceptions;
using Utils.Helpers;

namespace mediTool.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PacienteController : ControllerBase
    {
        private readonly IPacienteService _pacienteService;

        public PacienteController(IPacienteService pacienteService)
        {
            _pacienteService = pacienteService;
        }

        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var pacientes = await _pacienteService.GetAllAsync(null);
            return Ok(pacientes);
        }

        [HttpGet("vinculados")]
        public async Task<IActionResult> GetVinculados()
        {
            var pacientes = await _pacienteService.GetAllAsync(User.GetUserId());
            return Ok(pacientes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var paciente = await _pacienteService.GetByIdAsync(id);
            if (paciente == null)
            {
                return NotFound();
            }

            if (!User.IsAdmin() && !await _pacienteService.IsVinculadoAsync(id, User.GetUserId()))
                throw new ForbiddenError("No tiene permisos para acceder a este recurso.");

            return Ok(paciente);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PacienteCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var profesionalId = User.IsAdmin() ? null : (int?)User.GetUserId();
            var result = await _pacienteService.AddAsync(dto, profesionalId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PacienteUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!User.IsAdmin() && !await _pacienteService.IsVinculadoAsync(id, User.GetUserId()))
                throw new ForbiddenError("No tiene permisos para acceder a este recurso.");

            var result = await _pacienteService.UpdateAsync(id, dto);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [Authorize(Policy = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _pacienteService.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
