using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Service.Profesionales;
using Utils.DTOs.Profesional;
using Utils.Helpers;

namespace mediTool.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfesionalController : ControllerBase
    {
        private readonly IProfesionalService _profesionalService;

        public ProfesionalController(IProfesionalService profesionalService)
        {
            _profesionalService = profesionalService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            User.EnsureOwnership(id);
            var profesional = await _profesionalService.GetByIdAsync(id);
            if (profesional == null)
            {
                return NotFound();
            }
            return Ok(profesional);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProfesionalCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _profesionalService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProfesionalUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            User.EnsureOwnership(id);
            var result = await _profesionalService.UpdateAsync(id, dto);
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
            var success = await _profesionalService.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost("{id}/pacientes/{pacienteId}")]
        public async Task<IActionResult> VincularPaciente(int id, int pacienteId)
        {
            User.EnsureOwnership(id);
            await _profesionalService.VincularPacienteAsync(id, pacienteId);
            return Ok();
        }

        [HttpGet("{id}/pacientes")]
        public async Task<IActionResult> GetPacientesVinculados(int id)
        {
            User.EnsureOwnership(id);
            var pacientes = await _profesionalService.GetPacientesVinculadosAsync(id);
            return Ok(pacientes);
        }
    }
}
