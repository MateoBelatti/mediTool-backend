using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Service.Profesionales;
using Service.Turnos;
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
        private readonly ITurnoService _turnoService;

        public ProfesionalController(IProfesionalService profesionalService, ITurnoService turnoService)
        {
            _profesionalService = profesionalService;
            _turnoService = turnoService;
        }

        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var profesionales = await _profesionalService.GetAllAsync();
            return Ok(profesionales);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!User.IsAdmin())
            {
                User.EnsureOwnership(id);
            }
            var profesional = await _profesionalService.GetByIdAsync(id);
            if (profesional == null)
            {
                return NotFound();
            }
            return Ok(profesional);
        }

        [Authorize(Policy = "Admin")]
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

        [HttpDelete("{id}/pacientes/{pacienteId}")]
        public async Task<IActionResult> DesvincularPaciente(int id, int pacienteId)
        {
            User.EnsureOwnership(id);
            var success = await _profesionalService.DesvincularPacienteAsync(id, pacienteId);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("{id}/agenda")]
        public async Task<IActionResult> GetAgenda(int id, [FromQuery] DateTime desde, [FromQuery] DateTime hasta, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            User.EnsureOwnership(id);
            var turnos = await _turnoService.ObtenerAgenda(desde, hasta, page, pageSize, id);
            return Ok(turnos);
        }
    }
}
