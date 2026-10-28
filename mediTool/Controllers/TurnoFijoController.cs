using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Service.TurnosFijos;
using Utils.DTOs.TurnoFijo;
using Utils.Helpers;

namespace mediTool.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TurnoFijoController : ControllerBase
    {
        private readonly ITurnoFijoService _turnoFijoService;

        public TurnoFijoController(ITurnoFijoService turnoFijoService)
        {
            _turnoFijoService = turnoFijoService;
        }

        [HttpGet("profesional/{profesionalId}")]
        public async Task<IActionResult> GetByProfesional(int profesionalId)
        {
            User.EnsureOwnership(profesionalId);
            var turnos = await _turnoFijoService.ListarPorProfesional(profesionalId);
            return Ok(turnos);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearTurnoFijoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!User.IsAdmin())
                dto.ProfesionalId = User.GetUserId();

            var result = await _turnoFijoService.Crear(dto);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EditarTurnoFijoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _turnoFijoService.ObtenerPorId(id);
            if (existing == null) return NotFound();
            User.EnsureOwnership(existing.ProfesionalId);

            var result = await _turnoFijoService.Editar(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _turnoFijoService.ObtenerPorId(id);
            if (existing == null) return NotFound();
            User.EnsureOwnership(existing.ProfesionalId);

            await _turnoFijoService.Desactivar(id);
            return NoContent();
        }
    }
}
